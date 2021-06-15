aces_factory.py

#!/usr/bin/env python3
import argparse, os, re, random, string, json
import shutil
from hashlib import sha256

def zipdirectory(path_to_zip: str) -> str:
    print("attempting zip of " + path_to_zip)
    shutil.make_archive(path_to_zip + 'prepared', 'zip', path_to_zip)
    shutil.rmtree(path_to_zip)
    return path_to_zip + "prepared" + ".zip"

def watermark_file(path: str) -> int:
    print("-> Searching " + path + " for watermarkable lines...", end="")
    wf = open(path, "r")
    oldlines: list = wf.readlines()
    wf.close()
    newlines: list = list()
    lineno: int = 0
    watermarks: int = 0

    for line in oldlines:
        lineno += 1
        new = line
        if "aw:watermark" in line:
            print("ln" + str(lineno) + "...", end="")
            watermarks += 1
            new = line.replace("aw:watermark", "aw:" + watermark)
        newlines.append(new)

    if watermarks == 0:
        print("no watermarks found.")
    else:
        print("done.")

    wf = open(path, "w")

    for line in newlines:
        wf.write(line)

    wf.close()

    return watermarks

def generate_filelist(config: dict, directory: str) -> list:
    filelist: list() = list()
    m = config.get('method')
    if m == 'extension':
        pass
    elif m == 'namedfiles':
        for f in config.get('namedfiles'):
            filelist.append(f)
    return filelist

def en(s: str) -> bytes:
    return s.encode('utf-8')

def generate_watermark(email: str, asn_no: str):
    salt = ''.join(random.choice(string.ascii_lowercase) \
        for i in range(5))
    return sha256(en(asn_no) + en(email) + en(salt)).hexdigest()

def factory_create(directory: str, email: str, asn_no: str, existing_watermark: str):
    # Check validity of each parameter.
    if not os.path.isdir(directory):
        print("Error: directory does not exist.")
        return '', '', 0
    if not os.path.exists(directory + "/.acesconfig.json"):
        print("Error: directory does not contain a .acesconfig.json file.")
        return '', '', 0
    email_regex = re.compile(r"(^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$)")
    if not email_regex.match(email):
        print("Error: email argument is not an email address.")
        return '', '', 0

    if not len(asn_no) > 2:
        print("Error: asn_no must be greater than two characters.")
        return '', '', 0

    print("Preparing " + directory + " for " + email + " as " + asn_no)

    # Generate unique watermark code if none exists (first download)
    global watermark
    if existing_watermark == '':
        watermark = generate_watermark(email, asn_no)
        print("Generated watermark: " + watermark)
    else:
        watermark = existing_watermark

    # Open directory's .acesconfig.json file
    fconfig = open(directory + "/.acesconfig.json", "r")
    config: dict = json.loads(fconfig.read())
    fconfig.close()

    # Use config file to get list of markable files.
    markable_files: list = generate_filelist(config, directory)
    print("Method: " + config.get('method') + ", searching " + \
        str(len(markable_files)) + " file(s) for watermarkables.")

    total_marks: int = 0

    print("Attempting copytree...")
    copiedpath: str = "out/" + watermark[0:16] + "/" + asn_no

    # If folder already exists, delete old
    olddirectory: str = copiedpath[0:20]
    if os.path.exists(olddirectory): 
        shutil.rmtree(olddirectory)
    shutil.copytree(directory, copiedpath)

    for f in markable_files:
        if os.path.exists(copiedpath + "/" + f):
            total_marks += watermark_file(copiedpath + "/" + f)

    print("Total watermarks generated: " + str(total_marks))

    zipdir: str = zipdirectory(copiedpath)

    print("Created zipped folder at " + zipdir)

    return zipdir, watermark, total_marks

# Note that main() should only be called if testing this module
# individually. The actual main point of entry is factory_create(),
# as called by another module.
def main():
    parser = argparse.ArgumentParser(description="Watermark files specified in " + \
        "an .acesfile and zip the folder.")
    parser.add_argument("directory", type=str, help="directory with a .acesfile")
    parser.add_argument("email", type=str, help="student email for watermarking")
    parser.add_argument("asn_no", type=str, help="assignment identifier (i.e. Asn1)")
    args = parser.parse_args()

    factory_create(args.directory, args.email, args.asn_no)
    pass

if __name__ == "__main__":
    main()