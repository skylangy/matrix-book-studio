import os
from opencc import OpenCC

def convert_file_name():
    # Rename traditional Chinese file names to simplified Chinese file names
    source = r'C:\Users\Andy\Documents\Books\My Books\禁书'
    converter = OpenCC('t2s')

    for root, dirs, files in os.walk(source):
        for file in files:
            simplified_file_name = converter.convert(file)
            destination_file = os.path.join(root, simplified_file_name)
            if file != simplified_file_name and not os.path.exists(destination_file):
                print(f"Converting {file} to {simplified_file_name}")
                os.rename(os.path.join(root, file), destination_file)

def main():
    convert_file_name()

if __name__ == "__main__":
    main()