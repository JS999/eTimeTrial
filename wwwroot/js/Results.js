// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.


var currentRace = new Object();
var currentRaceId

var isNotReady = true;
function timerViewModel(currentRace) {
    self.Results = ko.observableArray([]);

    for (let i = 0; i < currentRace.teams.length; i++) {
       

        self.Results.push({
            TeamNumber: currentRace.teams[i].teamNumber,
            TeamName: currentRace.teams[i].teamName,
            StartTime: currentRace.teams[i].teamStartTime,
            FinishTime: currentRace.teams[i].teamFinishTime,
            IntervalOffSet: currentRace.teams[i].offSetMin, 
            Duration: currentRace.teams[i].teamDuration,
            Speed: currentRace.teams[i].speedKm
        });

        self.Results().sort((a, b) => b.Speed - a.Speed);
    }

    self.Download = function () {
        //await DownloadResults(self.Results());
        generateCSV(self.Results());
    };
    // Add a new person to the array
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
                    console.error("Error:", error);
                });

        } catch (error) {
            console.error('Error fetching data:', error);
        }
    }
    
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
        console.error('Error fetching data:', error);
    }
}

async function GetFromApi(url) {
    try {

        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        var data = await response.json();
        return data;
    } catch (error) {
        console.error('Error fetching data:', error);
    }
}










