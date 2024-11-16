from sys import argv

from markdown_parser import convert


def main():
    src_file = argv[1]
    dst_file = argv[2]
    convert(src_file, dst_file, pdf=dst_file.endswith('.pdf'))


if __name__ == '__main__':
    main()
