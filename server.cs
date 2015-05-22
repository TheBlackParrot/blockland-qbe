exec("./functions.cs");
exec("./support.cs");
exec("./item.cs");
exec("./minigame.cs");
exec("./commands.cs");

$Qbe::Version = "0.3.4-unstable-1";

if(!isObject(PublicClient))
{
	new AiConnection(PublicClient)
	{
		bl_id = 888888;
		name = "CONSOLE";
		isAdmin = 1;
		isSuperAdmin = 1;
	};
}

function initiateArena(%x,%y,%z)
{
	deleteVariables("$Qbe::Position*");

	if(!%x)
		%x = 10;
	if(!%y)
		%y = 10;
	if(!%z)
		%z = 10;

	if(isObject(QbeScores))
	{
		for(%i=0;%i<QbeScores.rowCount();%i++)
			QbeScores.removeRow(%i);
	}
	else
		new GuiTextListCtrl(QbeScores);

	for(%i=0;%i<$DefaultMinigame.numMembers;%i++)
		$DefaultMinigame.member[%i].score = 0;

	serverCmdClearAllBricks(PublicClient);

	initiateArenaStep2(%x,%y,%z);
}

function initiateArenaStep2(%x,%y,%z)
{
	if(getBrickCount() != 0)
	{
		schedule(100,0,initiateArenaStep2,%x,%y,%z);
		messageAll('',getBrickCount() SPC "bricks remain");
		return;
	}
	for(%cx=0;%cx<%x;%cx++)
		for(%cy=0;%cy<%y;%cy++)
			for(%cz=0;%cz<%z;%cz++)
				createBrick(%cx,%cy,%cz);

	initiateSpawn(%x,%y,%z);
}

function initiateSpawn(%x,%y,%z)
{
	$Qbe::Spawns = 0;

	%z = mFloor(%z/2);
	%x = %x+5;
	%y = %y+5;
	for(%cx=-5;%cx<%x;%cx++)
	{
		for(%cy=-5;%cy<%y;%cy++)
		{
			if(!$Qbe::Position[%cx*2 SPC %cy*2 SPC (%z*2)+800])
				createBrick(%cx,%cy,%z,1);
		}
	}

	for(%i=0;%i<$DefaultMinigame.numMembers;%i++)
	{
		%client = $DefaultMinigame.member[%i];
		if(!isObject(%client.player))
			%client.spawnPlayer();

		%pos = $Qbe::Spawn[getRandom($Qbe::Spawns)].position;
		%client.player.setTransform(getWords(%pos,0,1) SPC getWord(%pos,2)+3);
		%client.player.setVelocity("0 0 0");
	}
	initiateArena_Callback(getBrickCount());
}

function getColorType(%spawn)
{
	if(%spawn)
		return 6;
	%rand = getRandom(100);
	if(%rand >= 0 && %rand <= 10)
		return 0;
	if(%rand >= 11 && %rand <= 16)
		return 1;
	if(%rand >= 17 && %rand <= 20)
		return 2;
	if(%rand == 21 || %rand == 22)
		return 3;
	if(%rand == 23)
		return 4;
	return 8;
}

function createBrick(%x,%y,%z,%spawn)
{
	%type = getColorType(%spawn);
	%brick = new fxDTSBrick()
	{
		angleID = 1;
		colorFxID = 0;
		colorID = %type;
		dataBlock = "brick4xCubeData";
		isBaseplate = 1;
		isPlanted = 1;
		position = %x*2 SPC %y*2 SPC (%z*2)+800;
		printID = 0;
		shapeFxID = 0;
		stackBL_ID = 888888;
	};
	%brick.plant();
	%brick.setTrusted(1);
	BrickGroup_888888.add(%brick);

	switch(%type)
	{
		case 0:
			%brick.maxHealth = 10;
			%brick.type = "Red Block";
			%brick.value = 3;
		case 1:
			%brick.maxHealth = 15;
			%brick.type = "Yellow Block";
			%brick.value = 7;
		case 2:
			%brick.maxHealth = 25;
			%brick.type = "Green Block";
			%brick.value = 15;
		case 3:
			%brick.maxHealth = 40;
			%brick.type = "Blue Block";
			%brick.value = 35;
		case 4:
			%brick.maxHealth = 60;
			%brick.type = "White Block";
			%brick.value = 60;
		case 8:
			%brick.maxHealth = 4;
			%brick.type = "Neutral Block";
			%brick.value = 1;
	}
	if(!%spawn)
		%brick.health = %brick.maxHealth;
	else
	{
		$Qbe::Spawn[$Qbe::Spawns] = %brick;
		$Qbe::Spawns++;
	}
	$Qbe::Position[%x*2 SPC %y*2 SPC (%z*2)+800] = %brick;
}

function convertRGBToHex(%dec)
{
	%str = "0123456789ABCDEF";

	while(%dec != 0)
	{
		%hexn = %dec % 16;
		%dec = mFloor(%dec / 16);
		%hex = getSubStr(%str,%hexn,1) @ %hex;    
	}

	if(strLen(%hex) == 1)
		%hex = "0" @ %hex;
	if(!strLen(%hex))
		%hex = "00";

	return %hex;
}