import unittest
import scraper
import requests

class TestBCSOMethods(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        #Testing data has the dates from 01/01/2023 to 01/31/2023 with 1000 rows
        cls.HTMLFile = open("BCSOraw_testing_data.html", "r")
        cls.response = cls.HTMLFile.read()


    @classmethod
    def tearDownClass(cls) -> None:
        cls.HTMLFile.close()


    def test_bad_paramters(self):
        self.assertEqual(scraper.run_BSCO_scrape(None, "06/28/2023", "30"), None)
        self.assertEqual(scraper.run_BSCO_scrape("06/28/2023", None, "30"), None)
        self.assertEqual(scraper.run_BSCO_scrape("06/28/2023", "06/28/2023", None), None)
        self.assertEqual(scraper.run_BSCO_scrape("06/28/2023", "06/28/2023", 100000), None)
    
    def test_time_conversion(self):
        self.assertEqual(scraper.convert_time("12:00", "AM"), "00:00")
        self.assertEqual(scraper.convert_time("9:30", "AM"), "09:30")
        self.assertEqual(scraper.convert_time("11:00", "AM"), "11:00")
        self.assertEqual(scraper.convert_time("12:00", "PM"), "12:00")
        self.assertEqual(scraper.convert_time("1:00", "PM"), "13:00")

    def test_parsing(self):
        self.testing_data = scraper.parse_BCSO_response(TestBCSOMethods.response)
        self.assertEqual(self.testing_data[0].get('Incident ID'), '2023024748')
        

class TestCPDMethods(unittest.TestCase):
    
    def test_bad_parameters(self):
        self.assertEqual(scraper.run_CPD_scrape(None, None), None)
        self.assertEqual(scraper.run_CPD_scrape("2023-06-28", None), None)
        self.assertEqual(scraper.run_CPD_scrape("202-06-28", "2023-06-28"), None)
        self.assertEqual(scraper.run_CPD_scrape("2023-06-28", "2023/06/28"), None)
