"""Check shipped icon/template integrity without Unity or third-party Python modules."""
from pathlib import Path
import re
import struct
import unittest

ROOT = Path(__file__).resolve().parents[2]
BRAND = ROOT / 'unity/Assets/Kesho/Branding'
WEB = ROOT / 'unity/Assets/WebGLTemplates/Kesho'

class BrandingAssets(unittest.TestCase):
    def test_png_icon_and_browser_sizes(self):
        for path, expected in [(BRAND/'app-icon.png',1024),(WEB/'favicon.png',192)]:
            data=path.read_bytes()
            self.assertEqual(data[:8],b'\x89PNG\r\n\x1a\n')
            self.assertEqual(struct.unpack('>II',data[16:24]),(expected,expected))
    def test_ico_has_small_browser_sizes(self):
        data=(WEB/'favicon.ico').read_bytes(); reserved,kind,count=struct.unpack('<HHH',data[:6])
        self.assertEqual((reserved,kind,count),(0,1,3))
        sizes=[]
        for i in range(count):
            entry=data[6+i*16:22+i*16]; sizes.append((entry[0],entry[1]))
            length,offset=struct.unpack('<II',entry[8:]); self.assertGreater(length,0);self.assertLessEqual(offset+length,len(data))
        self.assertEqual(set(sizes),{(16,16),(32,32),(48,48)})
    def test_template_uses_same_original_mark_and_existing_icons(self):
        self.assertEqual((BRAND/'kesho.svg').read_bytes(),(WEB/'favicon.svg').read_bytes())
        html=(WEB/'index.html').read_text()
        for link in re.findall(r'href="(favicon\.[^"]+)"',html):self.assertTrue((WEB/link).is_file())
        self.assertIn('createUnityInstance',html)
        self.assertIn('{{{ LOADER_FILENAME }}}',html)
        self.assertIn('role="alert"',html)
    def test_baked_audio_containers_and_unity_metadata(self):
        for name in ['paper','bedding','campus']:
            path=ROOT/'unity/Assets/Kesho/Resources/Sound'/(name+'.ogg')
            self.assertEqual(path.read_bytes()[:4],b'OggS')
            self.assertTrue(Path(str(path)+'.meta').is_file())
        for path in BRAND.rglob('*'):
            if path.is_file() and path.suffix!='.meta': self.assertTrue(Path(str(path)+'.meta').is_file())

if __name__=='__main__':unittest.main()
