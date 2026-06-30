using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lostbyte.Toolkit.FactSystem.Nodes;

namespace Lostbyte.Toolkit.FactSystem.Editor
{
    public static class ConditionParser
    {
        public static INode Parse(string condition)
        {
            List<Tuple<string, string>> tokens = Tokenize(condition);
            if (tokens == null || tokens.Count == 0) return null;

            int position = 0;

            Tuple<string, string> Peek() => position < tokens.Count ? tokens[position] : new(null, null);

            INode ParsePrimary()
            {
                (var tokenType, var tokenValue) = Peek();
                switch (tokenType)
                {
                    case "LPAREN":
                        position++;
                        INode node = Parse();
                        if (Peek().Item1 != "RPAREN")
                            throw new Exception("Missing closing parenthesis");
                        position++;
                        return node;
                    case "FACT":
                        position++;
                        var match = Regex.Match(tokenValue, @"^(?<key>[a-zA-Z_]\w*)\[(?<fact>[a-zA-Z_]\w*)\]$");
                        if (!match.Success)
                            throw new Exception($"Invalid FACT token: {tokenValue}");

                        string keyName = match.Groups["key"].Value;
                        string factName = match.Groups["fact"].Value;
                        var key = keyName == "this" ? null : FactEditorUtils.GetKey(keyName);
                        var fact = FactEditorUtils.GetFact(factName);
                        if ((keyName != "this" && key == null) || fact == null)
                            throw new Exception($"Unknown key or fact");
                        return new FactNode() { Key = key, Fact = fact };
                    case "NUMBER":
                        position++;
                        return float.TryParse(tokenValue.Replace('.', ','), out var numResult) ? new NumericNode() { Value = numResult } : throw new Exception("Cannot parse value");
                    case "STRING":
                        position++;
                        return new StringNode() { Value = tokenValue };
                    case "BOOL":
                        position++;
                        return bool.TryParse(tokenValue, out var boolResult) ? new BoolNode() { Value = boolResult } : throw new Exception("Cannot parse value");
                    case "ENUM":
                        position++;
                        return new EnumNode() { ValueName = tokenValue };
                    default:
                        throw new Exception($"Unsupported token: {{{tokenType}: {tokenValue}}}");
                }
            }

            int GetPrecedence(string op) => op switch
            {
                "^" => 7,
                "*" or "/" or "%" => 6,
                "+" or "-" => 5,
                "<" or ">" or "<=" or ">=" => 4,
                "==" or "!=" => 3,
                _ => 0
            };

            INode ParseOP(int minPrecedence = 0)
            {
                INode node = ParsePrimary();
                while (Peek().Item1 == "OP")
                {
                    var op = Peek().Item2;
                    int opPrecedence = GetPrecedence(op);
                    if (opPrecedence < minPrecedence) break;
                    position++;
                    int nextMinPrecedence = op == "^" ? opPrecedence : opPrecedence + 1;
                    INode rNode = ParseOP(nextMinPrecedence);

                    if (node is EnumNode eNode1 && rNode is FactNode fNode1 && fNode1.Fact is EnumFactDefinition eFact1)
                    {
                        eNode1.Value = (Enum)Enum.Parse(eFact1.EnumType, eNode1.ValueName);
                        node = eNode1;
                    }
                    else if (rNode is EnumNode eNode2 && node is FactNode fNode2 && fNode2.Fact is EnumFactDefinition eFact2)
                    {
                        eNode2.Value = (Enum)Enum.Parse(eFact2.EnumType, eNode2.ValueName);
                        rNode = eNode2;
                    }
                    node = new OPNode() { LNode = node, RNode = rNode, Op = op };
                }
                return node;
            }

            INode ParseAnd()
            {
                INode node = ParseOP();
                while (Peek().Item1 == "AND")
                {
                    position++;
                    if (node is IBoolNode l && ParseOP() is IBoolNode r)
                        node = new AndNode() { LNode = l, RNode = r };
                    else throw new Exception("Type mismatch");
                }
                return node;
            }

            INode ParseOr()
            {
                INode node = ParseAnd();
                while (Peek().Item1 == "OR")
                {
                    position++;
                    if (node is IBoolNode l && ParseAnd() is IBoolNode r)
                        node = new OrNode() { LNode = l, RNode = r };
                    else throw new Exception("Type mismatch");
                }
                return node;
            }

            INode ParseStatement()
            {
                INode node = ParseOr();
                (var tokenType, var _) = Peek();

                if (tokenType == "ASSIGN")
                {
                    position++;
                    INode rightSide = ParseStatement();
                    if (node is not FactNode fNode) throw new Exception("The left side of an assignment must be a Fact");

                    if (rightSide is EnumNode eNode && fNode.Fact is EnumFactDefinition eFact)
                    {
                        eNode.Value = (Enum)Enum.Parse(eFact.EnumType, eNode.ValueName);
                        rightSide = eNode;
                    }

                    return new AssignNode() { Target = fNode, ValueNode = rightSide };
                }
                else if (tokenType == "ADD_ASSIGN")
                {
                    position++;
                    INode rightSide = ParseStatement();
                    if (node is not FactNode fNode) throw new Exception("The left side of += must be a Fact");
                    return new AddAssignNode() { Target = fNode, ValueNode = rightSide };
                }
                else if (tokenType == "SUB_ASSIGN")
                {
                    position++;
                    INode rightSide = ParseStatement();
                    if (node is not FactNode fNode) throw new Exception("The left side of -= must be a Fact");
                    return new SubAssignNode() { Target = fNode, ValueNode = rightSide };
                }
                else if (tokenType == "INC")
                {
                    position++;
                    if (node is not FactNode fNode) throw new Exception("The target of an increment must be a Fact");
                    return new IncrementNode() { Target = fNode };
                }
                else if (tokenType == "DEC")
                {
                    position++;
                    if (node is not FactNode fNode) throw new Exception("The target of a decrement must be a Fact");
                    return new DecrementNode() { Target = fNode };
                }
                return node;
            }

            INode Parse()
            {
                return ParseStatement();
            }

            INode root = Parse();
            root?.Validate();

            // if (root is FactNode fNode && fNode.Fact is not BoolFactDefinition && root is not IActionNode)
            //     throw new Exception("Only bool facts are allowed as root");

            return root;
        }

        private static List<Tuple<string, string>> Tokenize(string condition)
        {
            Dictionary<string, string> tokensMap = new()
            {
                // Core Types
                { "FACT", @"\b[a-zA-Z_]\w*\[[a-zA-Z_]\w*\]" },
                { "NUMBER", @"\b\d+([.,]\d+)?\b" },
                { "STRING", @"""[^""]*""" },                   
                
                // Logical keywords
                { "BOOL", @"\btrue\b|\bfalse\b" },
                { "AND", @"\band\b" },
                { "OR", @"\bor\b" },
                
                // Action Operators 
                { "INC", @"\+\+" },
                { "DEC", @"--" },
                { "ADD_ASSIGN", @"\+=" },
                { "SUB_ASSIGN", @"-=" },
                { "OP", @"==|!=|>=|<=|>|<|\+|-|\*|/|%|\^" },
                { "ASSIGN", @"=" },

                { "ENUM", @"\b[a-zA-Z_][a-zA-Z0-9_]*\b" }, 
                
                // Layout
                { "LPAREN", @"\(" },
                { "RPAREN", @"\)" },
                { "SKIP", @"\s+" }
            };

            string masterPattern = string.Join("|", tokensMap.Select(kvp => $"(?<{kvp.Key}>{kvp.Value})"));
            Regex regex = new(masterPattern);

            List<Tuple<string, string>> tokens = new();
            var matches = regex.Matches(condition);
            int currentIndex = 0;

            foreach (Match match in matches)
            {
                foreach (var key in tokensMap.Keys)
                {
                    if (match.Groups[key].Success)
                    {
                        if (key != "SKIP")
                        {
                            tokens.Add(new(key, match.Value));
                        }
                        break;
                    }
                }
                currentIndex += match.Length;
            }

            if (currentIndex != condition.Length)
            {
                throw new Exception($"Unrecognized trailing input at index {currentIndex}");
            }

            return tokens;
        }
    }
}