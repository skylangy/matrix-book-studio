import os
from PIL import Image

def convert():
    source = r'G:\Audio Books'

    for root, dirs, files in os.walk(source):
        for file in files:
            if file.lower().endswith('.jpg'):
                source_file = os.path.join(root, file)
                destination_file = os.path.splitext(source_file)[0] + '.png'

                if not os.path.exists(destination_file):
                    print(f"Converting {source_file} to {destination_file} ...")
                    with Image.open(source_file) as img:
                        img.save(destination_file)
                        print(f"Converted {source_file} to {destination_file}")

def main():
    convert()

if __name__ == "__main__":
    main()