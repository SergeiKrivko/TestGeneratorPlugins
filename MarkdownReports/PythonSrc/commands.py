import json
import os
import pathlib
import subprocess
import sys


def get_si():
    if os.name == 'nt':
        si = subprocess.STARTUPINFO()
        si.dwFlags |= subprocess.STARTF_USESHOWWINDOW
        return si
    else:
        return None


ENCODINGS = ['utf-8', 'utf-16', 'ansi', 'ascii', 'charmap', 'windows-1251']


def cmd_command(command, **kwargs):
    if isinstance(text := kwargs.get('input', b''), str):
        kwargs['input'] = text.encode(kwargs.get('encoding', 'utf-8'))
    print_args(command)
    res = subprocess.run(command, capture_output=True, startupinfo=get_si(), **kwargs)
    for encoding in ENCODINGS:
        try:
            res.stdout = res.stdout.decode(encoding)
            break
        except UnicodeDecodeError:
            pass
    else:
        raise UnicodeDecodeError
    for encoding in ENCODINGS:
        try:
            res.stderr = res.stderr.decode(encoding)
            break
        except UnicodeDecodeError:
            pass
    else:
        raise UnicodeDecodeError
    return res


def cmd_command_pipe(command, stdout=True, stderr=False, **kwargs):
    try:
        proc = subprocess.Popen(command, startupinfo=get_si(), text=True,
                                stdout=subprocess.PIPE if stdout else None,
                                stderr=subprocess.STDOUT if stderr and stdout else None,
                                **kwargs)
        for line in iter(proc.stdout.readline, ''):
            yield line
    except Exception as ex:
        raise subprocess.CalledProcessError(1, f"{ex.__class__.__name__}: {ex}")

    proc.stdout.close()
    exit_code = proc.poll()
    if exit_code:
        raise subprocess.CalledProcessError(exit_code, command)


def read_file(path, default=None) -> str:
    try:
        file = open(path, encoding='utf-8')
        res = file.read()
        file.close()
        return res
    except Exception as ex:
        if default is not None:
            return default
        raise ex


def read_binary(path, default=None) -> bytes:
    try:
        file = open(path, 'br', encoding='utf-8')
        res = file.read()
        file.close()
        return res
    except Exception as ex:
        if default is not None:
            return default
        raise ex


def read_json(path: str, expected_type: type = dict):
    try:
        res = json.loads(read_file(path, ''))
        if not isinstance(res, expected_type):
            res = expected_type()
    except json.JSONDecodeError:
        res = expected_type()
    return res


def write_file(path: str, text: str):
    with open(path, 'w', encoding='utf-8') as f:
        f.write(text)


def print_args(args: str):
    if args.startswith('wsl -e'):
        args = args[len('wsl -e'):]
        print(f"\033[32mwsl -e\033[0m", end=' ')
    i = 0
    for arg in args.split():
        if i == 0:
            print(f"\033[31m{arg}\033[0m", end=' ')
        elif arg.startswith('-'):
            print(f"\033[36m{arg}\033[0m", end=' ')
        else:
            print(f"\033[33m{arg}\033[0m", end=' ')
        i += 1
    print()


def is_text_file(path):
    try:
        with open(path, encoding='utf-8') as f:
            f.read()
            return True
    except UnicodeDecodeError:
        return False


def remove_files(path, extensions):
    if isinstance(extensions, str):
        extensions = [extensions]
    for file in os.listdir(path):
        for ex in extensions:
            if file.endswith(ex):
                os.remove(os.path.join(path, file))


def get_files(path: str, extensions: str | list[str]):
    if isinstance(extensions, str):
        extensions = [extensions]
    for root, dirs, files in os.walk(path):
        for file in files:
            for ex in extensions:
                if file.endswith(ex):
                    yield os.path.join(root, file)


def temp_path():
    return f"{os.path.dirname(os.path.dirname(__file__))}/temp"
