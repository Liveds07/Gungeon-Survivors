"""Assemble an offline Windows prototype from official downloads and wheels.

Usage: python tools/package_runtime.py --assets /path/to/downloads
Assets: python.nupkg (official python 3.12.10 NuGet package), bepinex.zip
(official Windows x64 5.4.23.5 release), wheels/ (CPython 3.12 Windows x64
UnityPy 1.25.4 dependencies), licenses/, Doorstop-4.5.0-source.zip.
No game files or extracted companion content may be added.
"""
import argparse
import hashlib
import json
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED

ROOT = Path(__file__).resolve().parents[1]
OFFICIAL_HASHES = {
    'python.nupkg': '0eb85c2dfccccf1b17352de4c397f69194035b7d37149eacc16f1147d93de3b8',
    'bepinex.zip': '82f9878551030f54657792c0740d9d51a09500eeae1fba21106b0c441e6732c4',
}


def trim_audio_imports(root):
    """Do not ship or load the unused proprietary FMOD audio runtime."""
    file = root / 'UnityPy/export/__init__.py'
    text = file.read_text()
    if '    AudioClipConverter,\n' not in text:
        raise ValueError('Unexpected UnityPy export layout; inspect before packaging')
    text = text.replace('    AudioClipConverter,\n', '').replace('    "AudioClipConverter",\n', '')
    file.write_text('# Modified for Gungeon Survivors: no eager audio imports.\n' + text)


def main(assets):
    for name, expected in OFFICIAL_HASHES.items():
        if hashlib.sha256((assets / name).read_bytes()).hexdigest() != expected:
            raise ValueError(f'{name}: unexpected download hash')
    wheels = sorted(p for p in (assets / 'wheels').glob('*.whl')
                    if not p.name.startswith(('fmod_toolkit-', 'pyfmodex-')))
    if len(wheels) != 11:
        raise ValueError('Expected all eleven sprite/data reader wheels')
    output = ROOT / 'distributions/Gungeon-Survivors-0.2.18-Windows-x64.zip'
    output.parent.mkdir(exist_ok=True)
    with tempfile.TemporaryDirectory() as temp:
        bundle = Path(temp) / 'Gungeon-Survivors-0.2.18'
        bundle.mkdir()
        with ZipFile(assets / 'python.nupkg') as archive:
            for name in archive.namelist():
                if name.startswith('tools/') and not name.endswith('/'):
                    target = bundle / 'runtime' / name.removeprefix('tools/')
                    target.parent.mkdir(parents=True, exist_ok=True)
                    target.write_bytes(archive.read(name))
        with ZipFile(assets / 'bepinex.zip') as archive:
            archive.extractall(bundle / 'loader')
        site = bundle / 'runtime/Lib/site-packages'
        subprocess.run([sys.executable, '-m', 'pip', 'install', '--no-compile',
                        '--no-deps', '--no-index', '--target', str(site),
                        '--platform', 'win_amd64', '--python-version', '312',
                        '--implementation', 'cp', '--abi', 'cp312',
                        '--only-binary=:all:', *map(str, wheels)], check=True)
        trim_audio_imports(site)
        for relative, target in [('build/GungeonSurvivors.dll', 'mod/GungeonSurvivors.dll'),
                                 ('tools/read_companion.py', 'tools/read_companion.py'),
                                 ('design/sheets.json', 'design/sheets.json')]:
            dest = bundle / target
            dest.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(ROOT / relative, dest)
        for name in ['Launch.cmd', 'Launch.ps1', 'START-HERE.txt']:
            shutil.copyfile(ROOT / 'packaging' / name, bundle / name)
        shutil.copytree(assets / 'licenses', bundle / 'licenses')
        shutil.copyfile(assets / 'Doorstop-4.5.0-source.zip',
                        bundle / 'licenses/Doorstop-4.5.0-source.zip')
        credits = (
            'Python 3.12.10: Python Software Foundation; official NuGet package.\n'
            'BepInEx 5.4.23.5: Bepis/BepInEx contributors; MIT license.\n'
            'Harmony/HarmonyX: Andreas Pardeike/BepInEx contributors.\n'
            'Mono.Cecil: Jb Evain and contributors. MonoMod: MonoMod contributors.\n'
            'Unity Doorstop 4.5.0: NeighTools contributors; LGPL-2.1; corresponding source included.\n'
            'UnityPy 1.25.4: K0lb3 and contributors; MIT; audio imports trimmed.\n'
            'Other Python dependency notices are retained in runtime/Lib/site-packages/*.dist-info.\n'
            'No game content is distributed. Original mod licensing/remix permissions remain unset.\n'
        )
        (bundle / 'licenses/CREDITS.txt').write_text(credits)
        entries = []
        for file in sorted(bundle.rglob('*')):
            if file.is_file():
                entries.append({'path': file.relative_to(bundle).as_posix(),
                                'size': file.stat().st_size,
                                'sha256': hashlib.sha256(file.read_bytes()).hexdigest()})
        manifest = {'version': '0.2.18', 'platform': 'windows-x64',
                    'windows_launch_tested': False, 'files': entries,
                    'wheel_inputs': {p.name: hashlib.sha256(p.read_bytes()).hexdigest() for p in wheels}}
        (bundle / 'manifest.json').write_text(json.dumps(manifest, indent=2))
        with ZipFile(output, 'w', ZIP_DEFLATED, compresslevel=9) as archive:
            for file in sorted(bundle.rglob('*')):
                if file.is_file():
                    archive.write(file, file.relative_to(bundle.parent).as_posix())
    print(output)
    print(f'{output.stat().st_size} bytes; SHA256 {hashlib.sha256(output.read_bytes()).hexdigest()}')


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--assets', type=Path, required=True)
    main(parser.parse_args().assets)
