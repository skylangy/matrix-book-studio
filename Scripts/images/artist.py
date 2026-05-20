import os

def copy_artist_images():
    # Copy artist images from the source directory to the destination directory
    source = r'C:\Users\Andy\Pictures\Books\authors'
    destination = r'G:\Audio Books'

    for root, dirs, files in os.walk(source):
        for file in files:
            if file.lower().endswith('.png'):
                file_name_without_extension = os.path.splitext(os.path.basename(file))[0]
                dest_folder = os.path.join(destination, file_name_without_extension, 'images')
                if not os.path.exists(dest_folder):
                    os.makedirs(dest_folder)


                source_file = os.path.join(root, file)
                destination_file = os.path.join(dest_folder, file)
                print(f"Copying {source_file} to {destination_file}")
                os.system(f'copy "{source_file}" "{destination_file}"')


def check_artist_images():


def main():
    copy_artist_images()

if __name__ == "__main__":
    main()