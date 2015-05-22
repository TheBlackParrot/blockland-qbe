function serverCmdStats(%client,%who)
{
	if(isObject(findClientByBL_ID(%who)))
		%who = findClientByBL_ID(%who);
	if(isObject(findClientByName(%who)))
		%who = findClientByName(%who);

	%rank = QbeScores.getRowNumByID(%who.bl_id)+1;
	if(!%rank)
		%rank = "N/A";
	else
		%rank = getRankFormat(%rank);

	messageClient(%client,'',"\c3Statistics for" SPC %who.name);
	messageClient(%client,'',"\c2Score:\c6" SPC formatNumber(%who.score));
	messageClient(%client,'',"\c2Rank:\c6" SPC %rank);
	messageClient(%client,'',"\c2Lifetime Score:\c6" SPC formatNumber(%who.totalScore));
	messageClient(%client,'',"\c2Deaths:\c6" SPC formatNumber(%who.deaths));
	messageClient(%client,'',"\c2Kills:\c6" SPC formatNumber(%who.kills));
	messageClient(%client,'',"\c2Wins:\c6" SPC formatNumber(%who.wins));
}

function serverCmdScores(%client)
{
	%position = QbeScores.getRowNumByID(%client.bl_id);
	%behind = %position+1;
	%ahead = %position-1;

	%current_row = QbeScores.getRowText(%position);
	%ahead_row = QbeScores.getRowText(%ahead);
	%behind_row = QbeScores.getRowText(%behind);

	if(%ahead_row !$= "")
	{
		if(getField(%ahead_row,2) $= "")
			%person = "ID" SPC getField(%ahead_row,0);
		else
			%person = getField(%ahead_row,2);
		messageClient(%client,'',"\c2" @ %ahead+1 @ ".\c6" SPC %person SPC "\c7--\c3" SPC getField(%ahead_row,1));
	}

	if(getField(%current_row,2) $= "")
		%person = "ID" SPC getField(%current_row,0);
	else
		%person = getField(%current_row,2);
	messageClient(%client,'',"\c2" @ %position+1 @ ".\c4" SPC %person SPC"\c7--\c3" SPC getField(%current_row,1));

	if(%behind_row !$= "")
	{
		if(getField(%behind_row,2) $= "")
			%person = "ID" SPC getField(%behind_row,0);
		else
			%person = getField(%behind_row,2);
		messageClient(%client,'',"\c2" @ %behind+1 @ ".\c6" SPC %person SPC "\c7--\c3" SPC getField(%behind_row,1));
	}
}