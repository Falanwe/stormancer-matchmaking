window.onload = main;

var matchmakingVM;

function main()
{
	matchmakingVM = new MatchmakingViewModel(parseInt(Math.random()*100));
	ko.applyBindings(matchmakingVM);
}
