"""Safety boundaries for the reader, using synthetic install markers, not game files."""
import tempfile
import unittest
from pathlib import Path
from tools.read_companion import read_content

class CompanionSafetyTests(unittest.TestCase):
    def test_missing_install_cannot_create_cache(self):
        with tempfile.TemporaryDirectory() as tmp:
            cache = Path(tmp) / 'cache'
            with self.assertRaisesRegex(ValueError, 'installed Vampire Survivors'):
                read_content(Path(tmp) / 'missing', 'unused', cache)
            self.assertFalse(cache.exists())

    def test_cache_cannot_write_into_install(self):
        with tempfile.TemporaryDirectory() as tmp:
            install = Path(tmp)
            (install / 'VampireSurvivors.exe').touch()
            (install / 'VampireSurvivors_Data').mkdir()
            (install / 'VampireSurvivors_Data/resources.assets').touch()
            with self.assertRaisesRegex(ValueError, 'outside the source project'):
                read_content(install, 'unused', install / 'cache')
            self.assertFalse((install / 'cache').exists())

    def test_cache_cannot_write_into_source(self):
        with tempfile.TemporaryDirectory() as tmp:
            install = Path(tmp)
            (install / 'VampireSurvivors.exe').touch()
            (install / 'VampireSurvivors_Data').mkdir()
            (install / 'VampireSurvivors_Data/resources.assets').touch()
            project = Path(__file__).resolve().parents[1]
            with self.assertRaisesRegex(ValueError, 'outside the source project'):
                read_content(install, 'unused', project / 'forbidden-cache')
            self.assertFalse((project / 'forbidden-cache').exists())
