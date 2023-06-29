#import time
import requests
import re
import csv
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

def convert_time(twelve_hour_format, cycle_of_day):
    hours_minutes = twelve_hour_format.split(":")
    if cycle_of_day == "AM":
        if len(hours_minutes[0]) == 2:
            if hours_minutes[0] == "12":
                return ("00" + ":" + hours_minutes[1]).strip()
            else:
                return twelve_hour_format.strip()
        else:
            return ("0" + twelve_hour_format).strip()
    else:
        if hours_minutes[0] == "12":
            return (twelve_hour_format).strip()
        else:
            return (str(int(hours_minutes[0]) + 12) + ":" + hours_minutes[1]).strip()   

def parse_BCSO_response(response):
    data = []

    soup = BeautifulSoup(response, "html.parser")
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
            call_time_matches = re.search(r"([0-9]{1,2}[:][0-9]{2}) ([A-Z]{2})", call_time)
            call_time = call_time_matches.group(1)
            cycle_of_day = call_time_matches.group(2)
            twenty_four_hour_call_time = convert_time(call_time, cycle_of_day)
            twenty_four_hour_call_time = twenty_four_hour_call_time.split(":")
            datetime_of_incident = datetime.datetime(int(day_month_year[2]), int(day_month_year[0]), int(day_month_year[1]), int(twenty_four_hour_call_time[0]), int(twenty_four_hour_call_time[1]))

            data.append({"Incident ID": incident_number, "Type": incident_type, "Address": address, "Date/Time of Incident": datetime_of_incident.strftime("%m/%d,%Y @ %H:%M")})
        x += 1
        

    return data

# Send a GET request to the webpage
def run_BSCO_scrape(startDate, endDate, rowCount):
    #Date format MM/DD/YYYY
    #Row count 1 < x < 10000
    #Will crash if more than 10000, the sheriffs website cannot handle the load
    if(startDate == None) or (endDate == None) or (rowCount == None) or ((re.search(r"([0-9]{2}[/][0-9]{2}[/][0-9]{4})", startDate) == None) or (len(startDate) > 10)) or ((re.search(r"([0-9]{2}[/][0-9]{2}[/][0-9]{4})", endDate) == None) or (len(endDate) > 10)) or (re.search(r"[0-9]{1,5}", str(rowCount)) == None) or (len(str(rowCount)) > 5):
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
    data = parse_BCSO_response(response)

    #num_of_incidents = len(data)
    #output_file = open("BCSOdbready.txt", "a")
    #for val in range(0, num_of_incidents):
    #    output_file.write(",".join((data[val].values())) + "\n")
    #output_file.close()


def send_and_store_request_CPD(startDate, endDate):
    session = requests.Session()
    agency_url = "https://www.como.gov/CMS/911dispatch/police.php"
    headers = {
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36",
    }
    parameters = {
        'type':'',
        'keyword':'',
        'Start_Date':startDate,
        'End_Date': endDate
    }

    session.post(agency_url, headers=headers, data=parameters)

    file_url = "https://www.como.gov/CMS/911dispatch/police_csvexport.php"

    data_file = open("CPDraw_data.txt", "a")

    
    download = session.get(file_url)

    decoded_content = download.content.decode('utf-8')

    cr = csv.reader(decoded_content.splitlines(), delimiter=',')
    my_list = list(cr)
    first_row = True
    for row in my_list:
        if not first_row:
            data_file.write(",".join(val for val in row) + "\n")
        else:
            first_row = False
            pass
    
    session.close()
    
    data_file.close()

def is_violent_CPD_crime(incident_type):
    for key in crimedict.cpddict.keys():
        if incident_type in key:
            return True
    return False

def map_from_CPD_incident_type(incident_type):
    for key in crimedict.cpddict.keys():
        if incident_type in key:
            return crimedict.cpddict.get(key)
    return None

def convert_CPD_time_to_datetime(raw_datetime):
    matches = re.search(r"([0-9]{1,2}[/][0-9]{1,2}[/][0-9]{1,4}) ([0-9]{1,2}[:][0-9]{1,2})[:][0-9]{2}[ ]([A-Z]{2})", raw_datetime)
    date = matches.group(1)
    time = matches.group(2)
    day_cycle = matches.group(3)

    time = convert_time(time, day_cycle)
    time = time.split(":")
    date = date.split("/")
    datetime_of_incident = datetime.datetime(int(date[2]), int(date[0]), int(date[1]), int(time[0]), int(time[1]))
    return datetime_of_incident

def format_stored_CPD_data():
    data = []
    data_file = open("CPDraw_data.txt", "r")

    for line in data_file:
        vals = line.split(",")
        if is_violent_CPD_crime(vals[3]):
            incident_date_time = convert_CPD_time_to_datetime(vals[1])
            data.append({"Incident ID": vals[0], "Type": map_from_CPD_incident_type(vals[3]), "Address": vals[2], "Date/Time of Incident": incident_date_time.strftime("%m/%d,%Y @ %H:%M")})
        else:
            pass

    data_file.close()
    return data

def run_CPD_scrape(startDate, endDate):
    #Format for startDate and endDate must be YYYY-MM-DD
    if (startDate == None) or (endDate == None) or ((re.search(r"([0-9]{4}[-][0-9]{2}[-][0-9]{2})", startDate) == None) or (len(startDate) > 10)) or ((re.search(r"([0-9]{4}[-][0-9]{2}[-][0-9]{2})", endDate) == None) or (len(endDate) > 10)):
        return None

    #send_and_store_request_CPD(startDate, endDate)
    data = format_stored_CPD_data()
    out_file = open("CPDprocesseddata.txt", "a")
    for incident in data:
        out_file.write(", ".join(incident.values()) + "\n")
    out_file.close()

    
            


#run_BSCO_scrape('01/01/2023', '01/31/2023', 1000)
#time.sleep(120)
#run_BSCO_scrape('02/01/2023', '02/28/2023', 10000)
#time.sleep(120)
#run_BSCO_scrape('03/01/2023', '03/31/2023', 10000)
#time.sleep(120)
#run_BSCO_scrape('04/01/2023', '04/30/2023', 10000)
#time.sleep(120)
#run_BSCO_scrape('05/01/2023', '05/31/2023', 10000)
run_CPD_scrape("2023-06-10", "2023-06-11")
