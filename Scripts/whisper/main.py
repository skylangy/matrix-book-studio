
from subtitle_aligner import SubtitleAligner

def main():
    base_path = r"G:\Audio Books\沈星星\边水往事" # r"G:\Audio Books\王友琴\文革受难者"
    name = "边水往事-第十二章 逃离金三角" # "文革受难者-第三章"

    aligner = SubtitleAligner()
    aligner.align(base_path, name)

if __name__ == "__main__":
    main()