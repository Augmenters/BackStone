#import time
import requests
import datetime
import crimedict
from bs4 import BeautifulSoup

def is_violent_BCSO_crime(incident_type):
    for key in crimedict.bcsodict.keys():
        if incident_type in key:
            return True
    return False

def map_from_BCSO_incident_type(query):
    for key in crimedict.bcsodict.keys():
        if query in key:
            return crimedict.bcsodict.get(key)
    return "Invalid"

def convert_time(twelve_hour_format):
    twenty_four_hour_format = ""
    if twelve_hour_format[-2:] == "AM":
        if len(twelve_hour_format) == 8:
            if twelve_hour_format[:2] == "12":
                return "00" + twelve_hour_format[3:6]
            else:
                return twelve_hour_format[:6]
        else:
            return "0" + twelve_hour_format[:6]
    else:
        hours_minutes = twelve_hour_format.split(":")
        return (str(int(hours_minutes[0]) + 12) + ":" + twelve_hour_format[1][:2])   

def parse_response(response):
    data = []

    soup = BeautifulSoup(response.text, "html.parser")
    incident_types = soup.findAll("td", attrs={"data-th": "Incident"})
    addresses = soup.findAll("td", attrs={"data-th": "Address"})
    call_dates = soup.findAll("td", attrs={"data-th": "Call Date"})
    call_times = soup.findAll("td", attrs={"data-th": "Call Time"})
    incident_numbers = soup.findAll("td", attrs={"data-th": "Incident #"})

    x = 0
    while x < len(incident_types):
        incident_types[x] = incident_types[x].getText().strip()
        if is_violent_BCSO_crime(incident_types[x]):
            incident_type = map_from_BCSO_incident_type(incident_types[x])
            address = addresses[x].getText().strip()
            call_date = call_dates[x].getText().strip()
            call_time = call_times[x].getText().strip()
            incident_number = incident_numbers[x].getText().strip()
            
            day_month_year = call_date.split("/")
            twenty_four_hour_call_time = convert_time(call_time)
            twenty_four_hour_call_time = twenty_four_hour_call_time.split(":")
            datetime_of_incident = datetime.datetime(int(day_month_year[2]), int(day_month_year[0]), int(day_month_year[1]), int(twenty_four_hour_call_time[0]), int(twenty_four_hour_call_time[1]))

            data.append({"Incident ID": incident_number, "Type": incident_type, "Address": address, "Date/Time of Incident": datetime_of_incident})
        x += 1
        

    return data

# Send a GET request to the webpage
def run_BSCO_scrape(startDate, endDate, rowCount):
    #Date format MM/DD/YYYY
    #Row count 1 < x < 10000
    #Will crash if more than 10000, the sheriffs website cannot handle the load
    if(startDate == "" or endDate == "" or rowCount == ""):
        return None
    url = 'https://report.boonecountymo.org/mrcjava/servlet/SH01_MP.I00070s'

    headers = {
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36",
    }

    # "" = view all
    incident_type = ""
    address = ""
    location = ''

    parameters = {
        'startDate' : startDate,
        'endDate' : endDate,
        'rls_EXTERNAL': 'CT',
        'val_EXTERNAL' : incident_type,
        'val_ADDR01' : address,
        'rls_CALCULA006': 'EQ',
        'val_CALCULA006' : location,
        'max_rows' : rowCount,
        'rls_CALLDATE': 'RG',
        'val_CALLDATE': '' + startDate + ' ' + endDate,
        'rls_CALLTIME': 'EQ',
        'val_CALLTIME': '',
        'rls_INNUM': 'EQ',
        'val_INNUM': '',
        'rls_REPORT': 'EQ',
        'val_REPORT': '',
        'rls_ADDR01': 'CT',
        'val_ADDR01': '',
        'rls_APTLOT': 'EQ',
        'val_APTLOT': '',
        'slnk': '0',
        'sort_col': 'CALLDATE',
        'sort_typ': '0',
        'cur_sort_col': '',
        'reorder': 'N',
        'pageName': '',
        'preview': '0',
        'debug': '0',
        'pageNum': '1',
        'total_pages': '',
        'total_rows': '',
        'ajax_form': '1',
        'auto_refresh': '0',
        'excel': '0',
        'use_udview': '0',
        'val_CALCULA006': '',
    }

    response = requests.post(url, headers=headers, data=parameters)
    
    data = parse_response(response)

    #num_of_incidents = len(data)
    #output_file = open("BCSOdbready.txt", "w")
    #for val in range(0, num_of_incidents):
    #    output_file.write()
    #output_file.close()

#run_BSCO_scrape('01/01/2023', '01/31/2023', 30)
#time.sleep(120)
#run_BSCO_scrape('02/01/2023', '02/28/2023', 10000)
#time.sleep(120)
#run_BSCO_scrape('03/01/2023', '03/31/2023', 10000)
#time.sleep(120)
#run_BSCO_scrape('04/01/2023', '04/30/2023', 10000)
#time.sleep(120)
#run_BSCO_scrape('05/01/2023', '05/31/2023', 10000)
