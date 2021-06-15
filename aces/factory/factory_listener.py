factory_listener.py

#!/usr/bin/env python3
"""
Very simple HTTP server in python for logging requests
Usage::
    ./server.py [<port>]
"""
from http.server import BaseHTTPRequestHandler, HTTPServer
from os import curdir, sep
import logging
import aces_factory as af
import json

class S(BaseHTTPRequestHandler):
    def _set_response(self):
        self.send_response(200)
        self.send_header('Content-type', 'text/html')
        self.end_headers()

    def do_GET(self):
        try:
            if self.path.endswith(".zip"):
                f = open(curdir + sep + self.path, 'rb')
                self.send_response(200)
                self.send_header('Content-type', 'application/zip')
                self.end_headers()
                self.wfile.write(f.read())
                f.close()
                # delete curdir if test successful: shutil.rmtree(dir_path)
                return

        except IOError:
            self.send_error(404, 'File Not Found: %s' % self.path)


    def do_POST(self):
        content_length = int(self.headers['Content-Length']) # <--- Gets the size of data
        post_data = self.rfile.read(content_length) # <--- Gets the data itself
        client_json: dict = json.loads(post_data.decode('utf-8'))
        logging.info("POST request,\nPath: %s\nHeaders:\n%s\n\nBody:\n%s\n", \
                str(self.path), str(self.headers), post_data.decode('utf-8'))

        self._set_response()
        server_json: dict = dict()
        server_json['zipped_directory'], server_json['watermark'], server_json['watermark_count'] = af.factory_create(client_json["directory"], \
            client_json["email"], client_json["asn_no"], client_json["existing_watermark"])
        self.wfile.write(json.dumps(server_json).encode('utf-8'))

def run(server_class=HTTPServer, handler_class=S, port=8080):
    logging.basicConfig(level=logging.INFO)
    server_address = ('', port)
    httpd = server_class(server_address, handler_class)
    logging.info('Starting httpd...\n')
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        pass
    httpd.server_close()
    logging.info('Stopping httpd...\n')

if __name__ == '__main__':
    from sys import argv

    if len(argv) == 2:
        run(port=int(argv[1]))
    else:
        run()
