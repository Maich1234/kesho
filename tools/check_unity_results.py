"""Fail a build when Unity's NUnit report is absent, empty, failed or inconclusive."""
import sys
import xml.etree.ElementTree as ET


def check_report(path):
    root = ET.parse(path).getroot()
    if root.tag != "test-run":
        raise ValueError("Expected a Unity NUnit test-run report")
    if root.get("result") != "Passed":
        raise ValueError("Unity test run did not pass: " + str(root.get("result")))
    if int(root.get("failed", "0")) or int(root.get("inconclusive", "0")):
        raise ValueError("Unity reported failed or inconclusive tests")
    if int(root.get("passed", "0")) < 1:
        raise ValueError("Unity ran no passing tests")
    return int(root.get("passed"))


if __name__ == "__main__":
    try:
        count = check_report(sys.argv[1])
    except (IndexError, OSError, ValueError, ET.ParseError) as error:
        print("Unity verification failed:", error, file=sys.stderr)
        sys.exit(1)
    print(f"Unity report verified: {count} tests passed")
