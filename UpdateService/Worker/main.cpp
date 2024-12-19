#include <iostream>
#include <filesystem>
#include <thread>
#include <chrono>
#include <stdexcept>
#include <cstdlib>

void CopyFile(const std::filesystem::path& srcPath, const std::filesystem::path& dstPath, int retryCount = 1) {
    for (int i = 0; i < retryCount; ++i) {
        try {
            std::cout << srcPath << " -> " << dstPath << std::endl;
            std::filesystem::copy(srcPath, dstPath, std::filesystem::copy_options::overwrite_existing);
            return;
        } catch (const std::filesystem::filesystem_error& e) {
            std::cout << e.what() << std::endl;
        }

        std::this_thread::sleep_for(std::chrono::seconds(1));
    }

    throw std::runtime_error("Failed to copy file after retries.");
}

void CopyFiles(const std::filesystem::path& srcPath, const std::filesystem::path& dstPath, int retryCount = 1) {
    std::cout << srcPath << " " << dstPath << std::endl;

    for (const auto& file : std::filesystem::directory_iterator(srcPath)) {
        if (std::filesystem::is_regular_file(file)) {
            CopyFile(file.path(), dstPath / file.path().filename(), retryCount);
        }
        else if (std::filesystem::is_directory(file)) {
            CopyFiles(file.path(), dstPath / file.path().filename(), retryCount);
        }
    }
}

int main(int argc, char* argv[]) {
    if (argc < 3) {
        std::cerr << "Usage: " << argv[0] << " <destination_path> <source_path>" << std::endl;
        return 1;
    }

    std::filesystem::path dstPath = argv[1];
    std::filesystem::path srcPath = argv[2];

    try {
        CopyFiles(srcPath, dstPath, 5);
        std::filesystem::remove_all(srcPath); // Удаление исходной директории рекурсивно

        // Запуск процесса
        std::string command = (dstPath / "TestGenerator").string();
        std::system(command.c_str());
    } catch (const std::exception& e) {
        std::cerr << "Error: " << e.what() << std::endl;
        return 1;
    }

    return 0;
}