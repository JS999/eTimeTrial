// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.


var currentRace = new Object();
var currentRaceId

var isNotReady = true;
function timerViewModel(currentRace) {
    var self = this;
    self.IsDisabled = ko.observable(true);
    self.isNotReady = ko.observable(isNotReady);
    self.currentRace = ko.observable(currentRace);
    self.currentRaceId = ko.observable(currentRace.id);

    self.TeamNumber = ko.observable("");
    self.TeamName = ko.observable("");
    self.TeamStartTime = ko.observable("");
    self.TeamFinishTime = ko.observable("");
    self.TeamDuration = ko.observable("");
    //self.FinishPosition = ko.observable("");

    self.Distance = ko.observable(currentRace.distanceKm);
    self.Interval = ko.observable(currentRace.interval);
    self.StartTime = ko.observable("");
    // Observable array to hold people

    self.Teams = ko.observableArray(currentRace.teams);
    self.Results = ko.observableArray([]);

    self.ToastName = ko.observable("");
    self.ToastDuration = ko.observable("");
    self.ToastMessage = ko.observable("");

    self.BtnClicked = function (btnNum) {

        self.TeamNumber(self.TeamNumber() + btnNum);
    };
    self.HeaderText = ko.computed(function () {
        return self.Results().length + "/" + self.Teams().length + " Completed";
    }, self);
    
    self.Start = async function () {
        try {
            self.IsDisabled(false);
            let startTime = await GetStartTime(0);

            if (!timerInterval) {
                stopwatchStartTime = Date.now() - elapsedTime;
                timerInterval = setInterval(() => {
                    elapsedTime = Date.now() - stopwatchStartTime;
                    document.getElementById('display').textContent = timeToString(elapsedTime);
                }, 10); // Update every 10ms for better precision
            }

            self.StartTime(startTime);
            self.currentRace.raceStart = startTime

            var params = '?raceId=' + self.currentRace().id + '&raceName=' + self.currentRace().raceName;
            var data = await GetFromApi('/api/TimeTrial/StartRace' + params);
        }
        catch (error) {
            showError("A network error occurred saving team starting time.\n" + error);
            console.error('Error fetching data:', error);
        }

      //  StartRace();
    };
    self.stopTimer = async function () {
        try {
            clearInterval(timerInterval);
            timerInterval = null;
            document.getElementById('display').textContent = "00:00:00.000";
            var params = '?raceId=' + self.currentRace().id + '&raceName=' + self.currentRace().raceName;
            var data = await GetFromApi('/api/TimeTrial/FinishRace' + params);
            window.location.href('/api/TimeTrial/Results' + params);
        }
        catch (error) {
            showError("A network error occurred saving Stopping time.\n" + error);
            console.error('Error fetching data:', error);
        }
    };

    self.pauseTimer = function () {
        if (timerInterval) {
            clearInterval(timerInterval);
            timerInterval = null;
        }
    };



    self.Download = function () {
        //await DownloadResults(self.Results());
        generateCSV(self.Results());
    };
    // Add a new person to the array
    self.TeamFinished = async function () {
        try {
        if (self.TeamNumber()) {

            var team = self.Teams().find(team => team.teamNumber == self.TeamNumber());


            if (team == null) {
                self.TeamNumber(""); // Clear input
                return;
            }
            const index = self.Teams().findIndex(obj => obj.teamNumber == self.TeamNumber());

            var minutes = 0;
            if (index > 0) {
                minutes = self.Interval() * index;
            }

            var teamStartTime = await GetTeamStartTime(self.StartTime(), minutes);
            var teamEndTime = await GetFinishTime();
            var duration = await GetDuration(teamStartTime, teamEndTime);
            var speed = await GetSpeed(self.Distance(), duration)

            var message;
            if (team.teamName == '') {
                message =  "<b>No Team Found</b>"
            }
            else {

                var teamResult = await SaveTeamResult(team);

                self.Results.push({
                    TeamNumber: teamResult.teamNumber,
                    TeamName: teamResult.teamName,
                    StartTime: teamStartTime,
                    FinishTime: teamEndTime,
                    IntervalOffSet: minutes,
                    Duration: duration,
                    Speed: speed
                });

                self.Results().sort((a, b) => b.Speed - a.Speed);
                self.TeamNumber(""); // Clear input


                message = '<b>' + team.teamName + "</b> Completed : <b>" + duration + "</b>";
                
            }
            
            self.ToastMessage(message);
            }
        } catch (error) {
            showError("A network error occurred saving team Finish time.\n" + error);
            console.error('Error fetching data:', error);
        }
    };
    async function SaveTeamResult() {
        try {
            var params = '?teamNumber=' + self.TeamNumber() + '&raceId=' + self.currentRace().id + '&raceName=' + self.currentRace().raceName

            var data = await GetFromApi('https://localhost:7219/api/TimeTrial/SaveTeamResult' + params)
            return data;

        } catch (error) {
            showError("A network error occurred saving team result.\n" + error);
            console.error('Error fetching data:', error);
        }
    }

    document.addEventListener('keydown', function (event) {
        if (self.IsDisabled() == true) return;
        if (event.delegateTarget != null) return event.key;
        console.log(`Key pressed: ${event.key}`);
        if (event.key === 'Enter') {
            console.log('Enter key was pressed!');
            TriggerToast();
        }
        if (event.key == "Enter") {
            self.TeamFinished();
        }
        if (event.key === '1' || event.key === '2' ||
            event.key === '3' || event.key === '4' ||
            event.key === '5' || event.key === '6' ||
            event.key === '7' || event.key === '8' ||
            event.key === '9' || event.key === '0') {
            console.log(event.key + ' key was pressed!');
            self.TeamNumber(self.TeamNumber() + event.key);

            var button = document.getElementById('btn' + event.key);
            if (button) {
                // Set focus to the button
                button.focus();
            }
        }

    });
    const toastTrigger = document.getElementById('liveToastBtn')
    const toastLiveExample = document.getElementById('liveToast')
    const toastBootstrap = bootstrap.Toast.getOrCreateInstance(toastLiveExample)

    if (toastTrigger) {
        toastTrigger.addEventListener('click', () => {
            toastBootstrap.show()
        })
    };



    function TriggerToast() {
        toastBootstrap.show();

    };

    async function GetDuration(startTime, endTime) {
        
        try {
            var params = '?startTime=' + startTime + '&endTime=' + endTime;

            var data = await GetFromApi('https://localhost:7219/api/TimeTrial/CalculateDuration' + params)
            return data;

        } catch (error) {
            showError("A network error occurred while getting duration.\n" + error);
            console.error('Error fetching data:', error);
        }

    }
    async function GetSpeed(distance, duration) {
        try {
            var params = '?distance=' + distance + '&duration=' + duration;

            var data = await GetFromApi('https://localhost:7219/api/TimeTrial/CalculateSpeed' + params)
            return data;

        } catch (error) {
            showError("A network error occurred while getting speed.\n" + error);
            console.error('Error fetching data:', error);
        }

    }
    async function GetStartTime(minOffSet) {

        try {
            var params = '?minutesOffset=' + minOffSet;

            var data = await GetFromApi('https://localhost:7219/api/TimeTrial/GetStartTime' + params)
            return data;

        } catch (error) {
            showError("A network error occurred while retrieving start time.\n" + error);
            console.error('Error fetching data:', error);
        }
    }
    async function GetFinishTime() {
        try {
            var data = await GetFromApi('https://localhost:7219/api/TimeTrial/GetFinishTime')
            return data;

        } catch (error) {
            showError("A network error occurred while retrieving Finish time.\n" + error);
            console.error('Error fetching data:', error);
        }

    }



    async function StartRace() {
        try {
            var params = '?raceId=' + self.currentRace.id + '&raceName=' + self.currentRace.raceName;
            const response = await fetch('/api/TimeTrial/StartRace' + params);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            var data = await response.json();
            return data;

        } catch (error) {
            showError("A network error occurred while starting race.\n" + error);
            console.error('Error fetching data:', error);
        }
    }
   
    async function DownloadResults(results) {

        try {

            await fetch('/api/TimeTrial/Download', {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(results),
            })
                .then((response) => {
                    if (!response.ok) {
                        throw new Error("Network response was not ok");
                    }
                    return response.json();
                })
                .then((data) => {
                    console.log("Success:", data);
                })
                .catch((error) => {
                    showError("A network error occurred while downloading results.\n" + error);
                    console.error("Error:", error);
                });

        } catch (error) {
            console.error('Error fetching data:', error);
        }
    }
    async function GetTeamStartTime(startTime, minOffset) {
        try {
            var params = '?startTime=' + startTime + '&minutesOffset=' + minOffset;
            var data = await GetFromApi('/api/TimeTrial/GetTeamStartTime' + params)
            return data;

        } catch (error) {
            showError("A network error occurred getting the Team Start Time.\n" + error);
            console.error('Error fetching data:', error);
        }
    }

    let stopwatchStartTime = 0;
    let elapsedTime = 0;
    let timerInterval;

    function timeToString(time) {
        const milliseconds = Math.floor((time % 1000));
        const seconds = Math.floor((time / 1000) % 60);
        const minutes = Math.floor((time / (1000 * 60)) % 60);
        const hours = Math.floor((time / (1000 * 60 * 60)) % 24);

        const formattedMilliseconds = milliseconds.toString().padStart(3, '0');
        const formattedSeconds = seconds.toString().padStart(2, '0');
        const formattedMinutes = minutes.toString().padStart(2, '0');
        const formattedHours = hours.toString().padStart(2, '0');

        return `${formattedHours}:${formattedMinutes}:${formattedSeconds}.${formattedMilliseconds}`;
    }

    function start() {
        if (!timerInterval) {
            stopwatchStartTime = Date.now() - elapsedTime;
            timerInterval = setInterval(() => {
                elapsedTime = Date.now() - stopwatchStartTime;
                document.getElementById('display').textContent = timeToString(elapsedTime);
            }, 10); // Update every 10ms for better precision
        }
    }
   

    function pauseTimer() {
        stop();
        elapsedTime = 0;
        document.getElementById('display').textContent = "00:00:00.000";
    }

    function formatDate(value) {

        if (value == '') return "00:00:00"
        var now = new Date();
        var nowDateTime = now.toISOString();
        var nowDate = nowDateTime.split('T')[0];
        var hms = value;
        var target = new Date(nowDate + 'T' + hms);


        // const date = new Date(value);
        return target.toLocaleTimeString('en-IE');// ('en-IE', {
        // year: 'numeric',
        // month: 'long',
        // day: 'numeric',
        // });
    }

    function generateCSV(results) {
        // function downloadCSV(data, filename = 'data.csv') {
        // Convert array of objects to CSV string
        const csvRows = [];
        const headers = Object.keys(results[0]);
        csvRows.push(headers.join(',')); // Add headers

        results.forEach(obj => {
            const row = headers.map(header => {
                const value = obj[header];
                return typeof value === 'string' ? `"${value.replace(/"/g, '""')}"` : value;
            });
            csvRows.push(row.join(','));
        });

        const csvString = csvRows.join('\n');

        // Create a Blob and trigger download
        const blob = new Blob([csvString], { type: 'text/csv' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'data.csv';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);

    };


   


   

}



window.onload = function () {
    document.getElementById("overlay").style.display = "block";
    var url = new URL(window.location);
    var raceId = url.searchParams.get("raceId");
    var raceName = url.searchParams.get("raceName");

    currentRaceId = raceId;
    currentRaceName = raceName;
    GetRace(currentRaceId, currentRaceName);
};

async function GetRace(currentRaceId, currentRaceName) {
    try {
        var params = '?raceId=' + currentRaceId + '&raceName=' + currentRaceName;

        var data = await GetFromApi('/api/TimeTrial/GetRace' + params)
        if (data.teams.length > 0) {
            isNotReady = false;
            document.getElementById("overlay").style.display = "none";
        }
        // Apply bindings
        const vm = new timerViewModel(data);
        ko.applyBindings(vm);


    } catch (error) {
        showError("A network error occurred while loading races.\n"+ error);
        console.error('Error fetching data:', error);
    }
}

async function GetFromApi(url) {
    
        const response = await fetch(url, { credentials: 'include' });
       
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        var data = await response.json();
        return data;
   
}










