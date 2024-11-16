# -*- mode: python ; coding: utf-8 -*-

import os


lib_path = ''
for root, _, _ in os.walk('venv'):
    if root.endswith('site-packages'):
        lib_path = root
        break


a = Analysis(
    ['main.py'],
    pathex=[],
    binaries=[],
    datas=[('MML2OMML.XSL', '.')],
    hiddenimports=[],
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=[],
    noarchive=False,
)
pyz = PYZ(a.pure)

exe = EXE(
    pyz,
    a.scripts,
    [],
    exclude_binaries=True,
    name='converter',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    console=True,
    disable_windowed_traceback=False,
    argv_emulation=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
)
