package QbeSupport
{
	function fxDTSBrick::killBrick(%this)
	{
		%this.schedule(1000,delete);
		%this.fakeKillBrick(getRandom(-5,5) SPC getRandom(-5,5) SPC getRandom(-5,5),1);
		%this.playSound(brickBreakSound);
		return;
		//parent::killBrick(%this);
	}

	function GuiTextListCtrl::addScore(%this,%client)
	{
		%old = %this.getRowText(0);
		if(%this.getRowNumByID(%client.getBLID()) != -1)
			%this.removeRowByID(%client.getBLID());
		%this.addRow(%client.getBLID(),%client.getBLID() TAB %client.score TAB %client.name TAB %client.wins,%this.rowCount()-1);
		%this.sortNumerical(1,0);
		if(getField(%old,0) != getField(%this.getRowText(0),0))
			messageAll('',"\c3" @ getField(%this.getRowText(0),2) SPC "\c5has taken the lead!");
	}

	function GameConnection::saveStats(%this)
	{
		%file = new FileObject();
		%file.openForWrite("config/server/Qbe/stats/" @ %this.getBLID());

		%file.writeLine(%this.wins);
		%file.writeLine(%this.kills);
		%file.writeLine(%this.deaths);
		%file.writeLine(%this.totalScore);

		%file.close();
		%file.delete();
	}

	function GameConnection::autoAdminCheck(%this)
	{
		%file = new FileObject();
		if(!isFile("config/server/Qbe/wins/" @ %this.getBLID()))
			return;
		%file.openForRead("config/server/Qbe/wins/" @ %this.getBLID());

		%this.wins = %file.readLine();
		%this.kills = %file.readLine();
		%this.deaths = %file.readLine();
		%this.totalScore = %file.readLine();

		%file.close();
		%file.delete();

		return parent::autoAdminCheck(%this);
	}

	function GameConnection::spawnPlayer(%this)
	{
		parent::spawnPlayer(%this);
		%this.HUDLoop();
		%this.giveQbeItems();

		%pos = $Qbe::Spawn[getRandom($Qbe::Spawns)].position;
		%this.player.setTransform(getWords(%pos,0,1) SPC getWord(%pos,2)+3);
	}

	function GameConnection::giveQbeItems(%this)
	{
		if($Qbe::RoundInProgress)
		{
			if(isObject(%this.player))
			{
				%this.player.tool[0] = QbeItem.getID();
				messageClient(%this,'MsgItemPickup','',0,QbeItem.getID());
				%this.player.tool[1] = GunItem.getID();
				messageClient(%this,'MsgItemPickup','',1,GunItem.getID());
				%this.player.tool[2] = SwordItem.getID();
				messageClient(%this,'MsgItemPickup','',2,SwordItem.getID());
			}
		}
	}

	function GameConnection::HUDLoop(%this)
	{
		cancel(%this.HUDLoopSched);
		%this.HUDLoopSched = %this.schedule(1000,HUDLoop);

		if(isObject(%this.player))
			%this.player.addHealth(1);

		if(!isObject(%this.player) || %this.HUDDisabled)
			return;

		%this.doHud();
	}

	function GameConnection::doHud(%this)
	{
		%health = mFloor(%this.player.getDataBlock().maxDamage - %this.player.getDamageLevel());
		%rank = QbeScores.getRowNumByID(%this.bl_id)+1;
		%score = formatNumber(%this.score);
		if($Qbe::RoundInProgress)
			%time = getTimeString(mFloor((getSimTime() - $Qbe::RoundStarted)/1000));
		else
			%time = "0:00";

		if(!%rank)
			%rank = "N/A";
		else
			%rank = getRankFormat(%rank);

		%this.bottomPrint("<just:center><font:Impact:24>\c6" @ %time @ "<br><just:left><font:Arial Bold:18>\c0Health:\c6" SPC %health @ "<just:right>\c3Score:\c6" SPC %score @ "<br><just:left>\c4Rank:\c6" SPC %rank,'',2);
	}

	function GameConnection::onClientLeaveGame(%this)
	{
		QbeScores.removeRowByID(%this.getBLID());
		return parent::onClientLeaveGame(%this);
	}

	function GameConnection::onDeath(%this,%obj,%killer,%type,%area)
	{
		echo(%this SPC %obj SPC %killer);
		if($Qbe::RoundInProgress)
		{
			%this.deaths++;

			if(%this == %killer || !%killer)
				return parent::onDeath(%this,%obj,%killer,%type,%area);

			%killer.kills++;

			%score_loss = mFloor(%this.score / 5);
			%this.score -= %score_loss;
			%killer.score += %score_loss;

			QbeScores.addScore(%this);
			QbeScores.addScore(%killer);
		}

		return parent::onDeath(%this,%obj,%killer,%type,%area);
	}

	// still contemplating on darkness
	//	-- see config/env1.cs for environment

	//function serverCmdLight(%client)
	//{
	//	messageClient(%client,'',"It's meant to be dark! Use the darkness to your advantage - scare your enemies!");
	//}

	function MinigameSO::startRound(%this)
	{
		$Qbe::RoundInProgress = 1;
		$Qbe::RoundStarted = getSimTime();
		$DefaultMinigame.multiplier = 1;
		for(%i=0;%i<%this.numMembers;%i++)
			%this.member[%i].giveQbeItems();
		$DefaultMinigame.roundEndSchedule = $DefaultMinigame.schedule(240000,endRound);
		$DefaultMinigame.mult2sch = $DefaultMinigame.schedule(60000,setMultiplier,2);
		$DefaultMinigame.mult3sch = $DefaultMinigame.schedule(120000,setMultiplier,3);
		$DefaultMinigame.mult4sch = $DefaultMinigame.schedule(180000,setMultiplier,4);
		$DefaultMinigame.mult5sch = $DefaultMinigame.schedule(220000,setMultiplier,5);
	}

	function MinigameSO::setMultiplier(%this,%value)
	{
		%this.multiplier = %value;
		switch(%value)
		{
			case 2 or 3 or 4:
				messageAll('',"\c3" @ %value-1 SPC "minute(s) in! \c5The Qbe hammer now does\c3" SPC %value @ "x damage \c5to bricks.");
			case 5:
				messageAll('',"\c320 seconds left! \c5The Qbe hammer now does\c3" SPC %value @ "x damage \c5to bricks.");
		}
	}

	function MinigameSO::endRound(%this)
	{
		$Qbe::RoundInProgress = 0;

		%winner = QbeScores.getRowText(0);
		%client = findClientByBL_ID(getField(%winner,0));
		%client.wins++;
		messageAll('',"\c3" @ getField(%winner,2) SPC "\c5has won this round with\c3" SPC formatNumber(getField(%winner,1)) SPC "points! \c5They have won\c3" SPC formatNumber(%client.wins) SPC "times.<br>\c5Resetting in 7 seconds...");
		$DefaultMinigame.schedule(7000,respawnAll);
		$StartNewRoundSchedule = schedule(6999,0,initiateArena,getRandom(25,40),getRandom(25,40),getRandom(25,40));

		for(%i=0;%i<%this.numMembers;%i++)
		{
			%client = %this.member[%i];
			%client.saveStats();
			if(!isObject(%client.player))
				continue;

			%client.camera.setOrbitMode(%client.player,%client.player.getTransform(),0.5,8,8,1);
			%client.camera.mode = "Orbit";
			%client.setControlObject(%client.camera);
		}
	}

	function onServerDestroyed(%a,%b)
	{
		$Qbe::Started = 0;
		cancel($StartNewRoundSchedule);
		cancel($DefaultMinigame.roundEndSchedule);
		cancel($DefaultMinigame.roundDelaySchedule);
		cancel($DefaultMinigame.mult2sch);
		cancel($DefaultMinigame.mult3sch);
		cancel($DefaultMinigame.mult4sch);
		cancel($DefaultMinigame.mult5sch);
		for(%i=0;%i<$DefaultMinigame.numMembers;%i++)
			$DefaultMinigame.member[%i].saveStats();
		return parent::onServerDestroyed(%a,%b);
	}
};
activatePackage(QbeSupport);