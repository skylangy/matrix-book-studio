import os


def scan():
    source = r'G:\Audio Books'

    count = 0
    mismatch_folders = set()

    for artist in next(os.walk(source))[1]:  # First-level directories
        artist_path = os.path.join(source, artist)
        for book in next(os.walk(artist_path))[1]:  # Subdirectories within each artist
            if book == 'images':
                continue

            book_images_folder = os.path.join(artist_path, book, 'images')
            if not os.path.exists(book_images_folder):
                # print(f"\tImages folder not found: {book_images_folder}")
                continue
            mp3_folder = os.path.join(artist_path, book, 'mp3')
            if not os.path.exists(mp3_folder):
                # print(f"\tMP3 folder not found: {mp3_folder}")
                continue

            images = set(os.listdir(book_images_folder))  # Use a set for faster lookup
            required_files = [
                f'{book}-wide-splash.png',
                f'{book}-wide-bg.png',
                # f'{book}-square-splash.png',
                # f'{book}-square-bg.png'
            ]


            for file_name in required_files:
                if file_name not in images:
                    count += 1
                    print(f"\t\t'{file_name}' not found in '{book_images_folder}'")
                    mismatch_folders.add(book_images_folder)

    print(f"{count} missing images found")

    for folder in sorted(mismatch_folders):
        print(f"\t{folder}")

    print(f"Find {len(mismatch_folders)} folders with mismatched images")


def main():
    scan()

if __name__ == "__main__":
    main()