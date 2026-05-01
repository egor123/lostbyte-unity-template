import http.server
import socketserver
import os

PORT = 8000
FOLDER_NAME = "WebGl"

script_dir = os.path.dirname(os.path.abspath(__file__))
target_dir = os.path.join(script_dir, FOLDER_NAME)

try:
    os.chdir(target_dir)
except FileNotFoundError:
    print(f"Error: Could not find the folder '{FOLDER_NAME}' in {script_dir}")
    print("Make sure your folder is named exactly 'WebGl' and is in the same place as this script.")
    exit()


class UnityHandler(http.server.SimpleHTTPRequestHandler):
    def guess_type(self, path):
        if path.endswith('.wasm') or path.endswith('.wasm.gz') or path.endswith('.wasm.br'):
            return 'application/wasm'
        if path.endswith('.js.gz') or path.endswith('.js.br'):
            return 'application/javascript'
        if path.endswith('.data.gz') or path.endswith('.data.br'):
            return 'application/octet-stream'
        return super().guess_type(path)

    def end_headers(self):
        if self.path.endswith('.gz'):
            self.send_header('Content-Encoding', 'gzip')
        elif self.path.endswith('.br'):
            self.send_header('Content-Encoding', 'br')

        super().end_headers()


with socketserver.TCPServer(("", PORT), UnityHandler) as httpd:
    print(f"Serving folder '{FOLDER_NAME}' at http://localhost:{PORT}")
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        pass
    finally:
        httpd.server_close()
