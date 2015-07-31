function MatchmakingViewModel(nbPlayersWaiting)
{
	this.nbPlayersWaiting = ko.observable(nbPlayersWaiting || 0);
	this.games = ko.observableArray();

	for (var i=0;i<10;i++)
	{
		var id = parseInt(Math.random()*1000);
		var nbPlayers = parseInt(Math.random()*10);
		this.games.push(new GameViewModel(id, "Game #"+id, nbPlayers, 10));
	}
}

function GameViewModel(id, name, nbPlayers, maxPlayers)
{
	this.nbPlayers = ko.observable(nbPlayers || 0);
	this.nbMaxPlayers = ko.observable(maxPlayers || 0);
	this.id = ko.observable(id || "");
	this.name = ko.observable(name || "");
}
