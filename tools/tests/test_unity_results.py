import importlib.util
from pathlib import Path
import tempfile
import unittest

spec = importlib.util.spec_from_file_location("unity_results", Path(__file__).parents[1] / "check_unity_results.py")
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)


class UnityResultsTests(unittest.TestCase):
    def report(self, text):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "results.xml"
            path.write_text(text)
            return module.check_report(path)

    def test_passing_report(self):
        self.assertEqual(5, self.report('<test-run result="Passed" passed="5" failed="0" inconclusive="0"/>'))

    def test_failure_even_when_process_exits_successfully(self):
        for text in ['<test-run result="Failed" passed="4" failed="1"/>',
                     '<test-run result="Passed" passed="4" failed="1"/>',
                     '<test-run result="Passed" passed="4" inconclusive="1"/>',
                     '<test-run result="Passed" passed="0"/>',
                     '<testsuites/>', '<test-run result="Passed" passed="invalid"/>']:
            with self.subTest(text=text), self.assertRaises(ValueError):
                self.report(text)

    def test_missing_and_truncated_reports(self):
        with tempfile.TemporaryDirectory() as directory:
            with self.assertRaises(OSError):
                module.check_report(Path(directory) / "missing.xml")
        with self.assertRaises(module.ET.ParseError):
            self.report('<test-run result="Passed"')


if __name__ == "__main__":
    unittest.main()
