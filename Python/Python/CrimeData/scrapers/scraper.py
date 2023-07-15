import time
import requests
from Python.CrimeData.scrapers import crimedict
import numpy
from bs4 import BeautifulSoup
from dateutil.relativedelta import relativedelta
from datetime import datetime
from Python.Database.models import Crime
from Python.Database.engine import insert_data

AGENCY_ID = 1

def grab_indice_from_wrapper(wrapper, index_desired):
    return [wrapper[0][index_desired], wrapper[1][index_desired], wrapper[2][index_desired], wrapper[3][index_desired]]


def in_BCSO_dict(query):
    for key in crimedict.bcsodict.keys():
        if query in key:
            return True
    return False


def get_BCSO_dict_value(query):
    correct_key = None
    for key in crimedict.bcsodict.keys():
        if query in key:
            correct_key = key
            break
    if correct_key is not None:
        return crimedict.bcsodict.get(correct_key)
    else:
        return "Invalid"


def convert_time(bad_time):
    if bad_time[-2:] == "AM" and bad_time[:2] == "12":
        return "00" + bad_time[2:-2]
    elif bad_time[-2:] == "AM":
        return "0" + bad_time[:-2]
    elif bad_time[-2:] == "PM" and bad_time[:2] != "12":
        if bad_time[:2] == "11":
            return "23" + bad_time[1:-2]
        else:
            twentyfour_time = str(int(bad_time[:1]) + 12)
            return twentyfour_time + bad_time[1:-2]
    else:
        return bad_time[:-2]


# Send a GET request to the webpage
def run_BSCO_scrape(startDate, endDate, rowCount, engine):
    #Date format MM/DD/YYYY
    #Row count 1 < x < 10000
    #Will crash if more than 10000, the sheriffs website cannot handle the load
    if(not startDate or not endDate or rowCount == ""):
        return None
    
    startDateStr = startDate.strftime('%m/%d/%Y')
    endDateStr = endDate.strftime('%m/%d/%Y')


    url = 'https://report.boonecountymo.org/mrcjava/servlet/SH01_MP.I00070s'

    headers = {
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36",
    }

    incidentType = ""
    # "" = view all
    address = ""
    location = ''

    parameters = {
        'startDate' : startDateStr,
        'endDate' : endDateStr,
        'rls_EXTERNAL': 'CT',
        'val_EXTERNAL' : incidentType,
        'val_ADDR01' : address,
        'rls_CALCULA006': 'EQ',
        'val_CALCULA006' : location,
        'max_rows' : rowCount,
        'rls_CALLDATE': 'RG',
        'val_CALLDATE': '' + startDateStr + ' ' + endDateStr,
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
        'total_pages': '13935',
        'total_rows': '418033',
        'ajax_form': '1',
        'auto_refresh': '0',
        'excel': '0',
        'use_udview': '0',
        'val_CALCULA006': '',
    }

    response = requests.post(url, headers=headers, data=parameters)
    soup = BeautifulSoup(response.text, "html.parser")
    incident_instance = soup.findAll("td", attrs={"data-th": "Incident"})
    addresses = soup.findAll("td", attrs={"data-th": "Address"})
    call_dates = soup.findAll("td", attrs={"data-th": "Call Date"})
    call_times = soup.findAll("td", attrs={"data-th": "Call Time"})
    incident_numbers = soup.findAll("td", attrs={"data-th": "Incident #"})

    #Filter out the instances we do not want
    x = 0
    indicies_to_delete = []
    while x < len(incident_instance):
        incident_instance[x] = incident_instance[x].getText().strip()
        if in_BCSO_dict(incident_instance[x]):
            incident_instance[x] = get_BCSO_dict_value(incident_instance[x])
        else:
            indicies_to_delete.append(x)
        addresses[x] = addresses[x].getText().strip()
        call_dates[x] = call_dates[x].getText().strip()
        call_times[x] = call_times[x].getText().strip()
        incident_numbers[x] = incident_numbers[x].getText().strip()
        x += 1

    temp_incidents_instance = numpy.array(incident_instance)
    temp_addresses = numpy.array(addresses)
    temp_cd = numpy.array(call_dates)
    temp_ct = numpy.array(call_times)
    temp_in = numpy.array(incident_numbers)

    incident_instance = numpy.delete(temp_incidents_instance, indicies_to_delete).tolist()
    addresses = numpy.delete(temp_addresses, indicies_to_delete).tolist()
    call_dates = numpy.delete(temp_cd, indicies_to_delete).tolist()
    call_times = numpy.delete(temp_ct, indicies_to_delete).tolist()
    incident_numbers = numpy.delete(temp_in, indicies_to_delete).tolist()

    call_datetimes = []
    for x in range(0, len(call_dates)):

        call_date = call_dates[x]
        call_time = call_times[x]

        # Combine date and time into a single string
        call_datetime_string = f"{call_date} {call_time}"

        # Convert the combined string into a datetime object
        call_datetime = datetime.strptime(call_datetime_string, '%m/%d/%Y %I:%M %p')

        call_datetimes.append(call_datetime)

    incident_wrapper = [call_datetimes, addresses, incident_instance, incident_numbers]
    num_of_incidents = len(incident_wrapper[0])

    crimes = []    
    for index in range(0, num_of_incidents):

        # TODO: Get agency id from db
        # TODO: Convert time to timeslot id
        crime = {
            "incident_id": incident_numbers[index],
            "agency_id": AGENCY_ID,
            "time_slot_id": 1,
            "type": incident_instance[index],
        }
        crimes.append(crime)

    unique_key = ["agency_id", "incident_id"]

    insert_data(engine, crimes, Crime, natural_key=unique_key)

    # # TODO: Get crime id from returned ids of crime insert
    # crime_address = {
    #     "crime_id": 1,
    #     "address": addresses[index]
    # }

    

def collect_BSCO_crime_data(start_date, end_date, engine):

    row_count = 10000

    while start_date < end_date:

        month_ahead_date = start_date + relativedelta(months=1)

        if end_date < month_ahead_date:

            print(f"Collecting data from {start_date.strftime('%m/%d/%Y'),} to {end_date.strftime('%m/%d/%Y'),}")
            run_BSCO_scrape(start_date, end_date, row_count, engine)
            start_date = end_date

        else:
            print(f"Collecting data from {start_date.strftime('%m/%d/%Y'),} to {month_ahead_date.strftime('%m/%d/%Y'),}")
            run_BSCO_scrape(start_date, month_ahead_date, row_count, engine)
            start_date = month_ahead_date

    

# run_BSCO_scrape('01/01/2023', '01/31/2023', 10000)
#time.sleep(120)
#run_BSCO_scrape('02/01/2023', '02/28/2023', 10000)
#time.sleep(120)
#run_BSCO_scrape('03/01/2023', '03/31/2023', 10000)
#time.sleep(120)
#run_BSCO_scrape('04/01/2023', '04/30/2023', 10000)
#time.sleep(120)
#run_BSCO_scrape('05/01/2023', '05/31/2023', 10000)
