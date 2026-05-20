import eyed3

# Load the MP3 file
file  = r"C:\Users\Andy\Documents\Music\Eason Friends 903ID Club.拉阔音乐会\Eason 18首选.首选陈奕迅\03.-.当这地球没有花.-.陈奕迅.-.Eason 18首选-首选陈奕迅.mp3"
audiofile = eyed3.load(file)

# Print general tag info
if audiofile.tag:
    print("Version:", audiofile.tag.version)
    print("Title:", audiofile.tag.title)
    print("Artist:", audiofile.tag.artist)
    print("Album:", audiofile.tag.album)
    print("Year:", audiofile.tag.getBestDate())
    print("Genre:", audiofile.tag.genre)

    # Print any comments (ID3v2 tags support comments)
    for comment in audiofile.tag.comments:
        print("Comment:", comment.text)