import os
from PIL import Image

def convert():
    source = r'I:\Source\封面\png'

    for root, dirs, files in os.walk(source):
        for file in files:
            if file.lower().endswith('.png'):
                source_file = os.path.join(root, file)
                destination_file = os.path.splitext(source_file)[0] + '.jpg'

                if not os.path.exists(destination_file):
                    try:
                        print(f"Converting {source_file} to {destination_file} ...")
                        with Image.open(source_file) as img:
                            rgb_img = img.convert('RGB')
                            rgb_img.save(destination_file, 'JPEG', quality=90)
                        print(f"Converted {source_file} to {destination_file}")
                    except Exception as e:
                        print(f"Failed to convert {source_file}: {e}")

def main():
    convert()

if __name__ == "__main__":
    main()