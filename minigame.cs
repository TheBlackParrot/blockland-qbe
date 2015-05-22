function startQbe()
{
	if($Qbe::Started)
	{
		messageAll('',"ERROR: Qbe has already started!");
		return;
	}
	$Qbe::Started = 1;
	$Qbe::RoundInProgress = 0;
	initiateArena(getRandom(25,40),getRandom(25,40),getRandom(25,40));
}
schedule(4000,0,startQbe);
messageAll('',"Running startQbe() in 4 seconds...");

function initiateArena_Callback(%brickcount)
{
	%delay = mCeil(mCeil(%brickcount/10)*5.5);
	messageAll('',"\c5The round will start in approximately\c3" SPC mRound(%delay/1000) SPC "seconds");

	$DefaultMinigame.roundDelaySchedule = $DefaultMinigame.schedule(%delay,startRound);
}

