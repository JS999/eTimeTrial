


function AppViewModel() {
    const modal = new bootstrap.Modal(document.getElementById("LoadTeamsModal"));
    const openBtn = document.getElementById("LoadTeamsBtn");
    openBtn.addEventListener("click", () => {
        modal.show();
    });


    var self = this;
    self.currentRace = ko.observable();
    self.url = ko.observable("/Timer"),
        self.NewRaceName = ko.observable("");

    self.NewRaceMonth = ko.observable();
    self.NewRaceDay = ko.observable();

    self.NewRaceHour = ko.observable();
    self.NewRaceMinute = ko.observable();



    self.NewRaceDistance = ko.observable(0);
    self.NewStartInverval = ko.observable(0);

    self.NewTeamNumber = ko.observable("");
    self.NewTeamName = ko.observable("");

    self.TeamNumber = ko.observable("");
    self.TeamName = ko.observable("");
    self.TeamStartTime = ko.observable();
    self.TeamFinishTime = ko.observable();
    self.TeamDuration = ko.observable();

    self.Distance = ko.observable(15);
    self.Interval = ko.observable(1);
    self.StartTime = ko.observable("");
    // Observable array to hold people

    self.Teams = ko.observableArray([]);
    self.Results = ko.observableArray([]);

    self.UpcomingRaces = ko.observableArray([]);
    self.CompletedRaces = ko.observableArray([]);


    self.ToastName = ko.observable("");
    self.ToastDuration = ko.observable("");
    self.ToastMessage = ko.observable("");

    self.timerUrl = ko.observable("#");// ko.computed(function () {


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
    self.SelectRace = function (id) {

        alert(id);
    };
    self.OpenLoadTeamsModal = function () {
        var myModalEl = document.getElementById('LoadTeamsModal');
        var modal = bootstrap.Modal.getInstance(myModalEl);
        // Close the modal
        modal.show();
    }
    self.AddTeams = function () {
        const currentDate = new Date();
        const currentYear = currentDate.getFullYear();
        var teamStartDate = new Date(currentYear, self.NewRaceMonth(), self.NewRaceDay(), self.NewRaceHour(), self.NewRaceMinute(), 0)

        self.Teams.push({
            TeamNumber: self.NewTeamNumber(),
            TeamName: self.NewTeamName(),
            StartPosition: self.Teams().length + 1,
        });
        self.NewTeamNumber("");
        self.NewTeamName("");
    };

    self.GotoTimer = function () {
        location.href = self.timerUrl;

    };

    self.SaveSettings = function () {
        const currentDate = new Date();
        const currentYear = currentDate.getFullYear();
       // var fullRaceDate = new Date(currentYear, self.NewRaceMonth(), self.NewRaceDay(), self.NewRaceHour(), self.NewRaceMinute(), 0)

       
        var newRace = {
            RaceName: self.NewRaceName(),
            DistanceKm: parseInt(self.NewRaceDistance(), 10),
            RaceStartDate: currentYear + '-' + self.NewRaceMonth() + '-' + self.NewRaceDay(),
            RaceStartTime: self.NewRaceHour() + ':' + self.NewRaceMinute() + ':00' ,
            Interval: parseInt(self.NewStartInverval(), 10),
            Teams: self.Teams()
        }

        NewRace(newRace);

        var message = '<b>' + self.Teams().length + "</b> Teams Loaded ";
        self.ToastMessage(message);
        TriggerToast();

        modal.hide();
    };
    document.getElementById('formFileSm').addEventListener('change', function (event) {
        const file = event.target.files[0];
        const reader = new FileReader();
        reader.onload = function (e) {
            const text = e.target.result;
            const rows = text.split('\n'); // Split by lines
            const headers = rows[0].split(','); // Extract headers
            const data = rows.slice(1).map(row => {
                const values = row.split(',');
                return headers.reduce((obj, header, index) => {
                    obj[header.trim()] = values[index].trim();
                    return obj;
                }, {});
            });
            /* self.Teams(data);*/

            var x = data;

            for (let i = 0; i < data.length; i++) {
                //console.log("The number is " + i);
                self.Teams.push({
                    TeamNumber: data[i].TeamNumber,
                    TeamName: data[i].TeamName,
                    StartPosition: self.Teams().length + 1,
                });
            }




            console.log(data); // Parsed data as an array of objects
        };
        reader.readAsText(file);
    });

    



    async function NewRace(raceSettings) {
        try {

            await fetch('/api/TimeTrial/NewRace', {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(raceSettings),
            })
                .then((response) => {
                    if (!response.ok) {
                        throw new Error("Network response was not ok");
                    }
                    return response.json();
                })
                .then((data) => {
                    self.currentRace = data
                    // self.timerUrl = "/Timer?raceId=" + self.currentRace.id + "&raceName=" + raceSettings.RaceName;

                    console.log("Success:", data);
                })
                .catch((error) => {
                    console.error("Error:", error);
                });

        } catch (error) {
            showError("A network error occurred saving new race.\n" + error);
            console.error('Error fetching data:', error);
        }
    }
    async function GetUpcomingRaces() {
        try {

            const response = await fetch('/api/TimeTrial/GetUpComingRaces', { credentials: 'include' });
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            var data = await response.json();
            self.UpcomingRaces(data);
            return data;
        }
         catch (error) {
                showError("A network error occurred getting upcoming races.\n" + error);
                console.error('Error fetching data:', error);
            }



       
    }
    window.onload = function () {
        GetUpcomingRaces();
    };

}

// Apply bindings
ko.applyBindings(new AppViewModel());












