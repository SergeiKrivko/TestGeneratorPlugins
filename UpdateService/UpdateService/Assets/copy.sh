#!/bin/bash

# Функция для копирования файла с повторными попытками
copy_with_retries() {
    local src_file="$1"
    local dst_file="$2"
    local retries=4
    local count=0

    while [ $count -lt $retries ]; do
        cp -f "$src_file" "$dst_file" && return 0
        echo "Не удалось скопировать $src_file. Попытка $((count + 1)) из $retries..."
        count=$((count + 1))
        sleep 1
    done

    echo "Не удалось скопировать $src_file после $retries попыток."
    return 1
}

# Функция для рекурсивного копирования файлов
copy_files_recursively() {
    local src_dir="$1"
    local dst_dir="$2"

    # Создаем целевую директорию, если она не существует
    mkdir -p "$dst_dir"

    # Копируем файлы
    for src_file in "$src_dir"/*; do
        if [ -d "$src_file" ]; then
            # Если это директория, рекурсивно копируем её содержимое
            local subdir_name=$(basename "$src_file")
            copy_files_recursively "$src_file" "$dst_dir/$subdir_name"
        else
            # Если это файл, копируем его с попытками
            copy_with_retries "$src_file" "$dst_dir/$(basename "$src_file")"
        fi
    done
}

# Проверка аргументов
if [ "$#" -ne 2 ]; then
    echo "Использование: $0 <исходная_папка> <целевое_место>"
    exit 1
fi

src_folder="$1"
dst_folder="$2"

# Запуск рекурсивного копирования
copy_files_recursively "$src_folder" "$dst_folder"

"$dst_folder"/TestGenerator
