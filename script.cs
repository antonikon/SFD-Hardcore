public static Dictionary<string, int> UserAccessList = new Dictionary<string, int>();

public static Random rnd = new Random();
public static IGame GlobalGame;
public static List<TLevel> LevelList = new List<TLevel>();
public static List <TMapPart> MapPartList = new List <TMapPart>();
public static List<TPlayer> PlayerList = new List<TPlayer>();
public static List<TEquipmentSlot> EquipmentList = new List<TEquipmentSlot>();
public static List<TPlayerMenu> PlayerMenuList = new List<TPlayerMenu>();
public static IObjectText BeginTimer;
public static int TimeToStart = 60;
public static int AreaTime = 60 * 2 + 5;
public static int GameState = 0;
public static Random GlobalRandom = new Random();
public static bool IsDebug = false;

public static IObjectTrigger UpdateTrigger;
public static IObjectTimerTrigger BeginTimerTrigger;

public static int CurrentMapPartIndex = 1;
public static Vector2 CameraPosition;
public static float CameraSpeed = 2.0f;

public static int MaxCapturePointProgress = 100;
public static int CapturePointRadius = 35;

//bleeding
public static float EasyBleeding = (float)0.01;
public static float HardBleeding = (float)0.015;
public static float JumpBleeding = (float)2.5;
public static int BleedingEffectPeriod = 100;

//diving
public static float DivingDamageFactor = 0.25f;
public static float MinDivingHeight = 50;

//air
public static int WorldTop = 300;
public static List <TPlayerStrikeInfo> AirPlayerList = new List <TPlayerStrikeInfo>();

//data
public static string OtherData = "";

//other
public static List <IObject> ObjectToRemove = new List <IObject>();
public static float MaxSlowSpeed = 1;
public static List <TEffect> EffectList = new List <TEffect>();
public static float XPBonus = 1f;

//electinoc warfare
public static int[] TeamJamming = {0, 0, 0};
public static int[] TeamHacking = {0, 0, 0};

//turrets
public static List<TTurret> TurretList = new List<TTurret>();
public static Dictionary<string, int> VisionObjects = new Dictionary<string, int>();

//shield generators
public static List<TShieldGenerator> ShieldGeneratorList = new List <TShieldGenerator>();

//drones
public static Vector2 DroneAreaBegin = new Vector2(-1168, -272);
public static Vector2 DroneAreaSize = new Vector2(308, 150);
public static List <List<int> > DroneMap1x1 = new List <List<int> >();


//weapon
public static Dictionary<WeaponItem, string> WeaponItemNames = new Dictionary<WeaponItem, string>();
public static List<TWeapon> WeaponTrackingList = new List<TWeapon>();
public static List<TThrownWeapon> ThrownTrackingList = new List<TThrownWeapon>();
public static List<WeaponItem> ExtraAmmoWeapon = new List<WeaponItem> (new WeaponItem[] {WeaponItem.PISTOL, WeaponItem.SHOTGUN, WeaponItem.SMG, WeaponItem.TOMMYGUN, WeaponItem.CARBINE, WeaponItem.ASSAULT, WeaponItem.SAWED_OFF, WeaponItem.UZI, WeaponItem.SILENCEDPISTOL, WeaponItem.SILENCEDUZI});
public static List<WeaponItem> ExtraExplosiveWeapon = new List<WeaponItem> (new WeaponItem[] {WeaponItem.GRENADE_LAUNCHER, WeaponItem.GRENADES, WeaponItem.MOLOTOVS, WeaponItem.MINES, WeaponItem.FLAREGUN, WeaponItem.FLAMETHROWER, WeaponItem.BAZOOKA});
public static List<WeaponItem> ExtraHeavyAmmoWeapon = new List<WeaponItem> (new WeaponItem[] {WeaponItem.REVOLVER, WeaponItem.SNIPER, WeaponItem.MAGNUM, WeaponItem.M60});

//missile
public static List<string> AllowedMissile = new List<string>(new string[] {"WpnKnife", "WpnKatana", "WpnAxe", "WpnShurikenThrown"});

public void AddUserAccessLevels() {
	//UserAccessList.Add("Admin", 2);
}

public class TThrownWeapon {
	public int Id = 0;
	public IObject Object = null;
	public WeaponItem Weapon = WeaponItem.NONE;
	public Vector2 Position = new Vector2(0, 0);
	public bool IsDestroyed = false;
	public bool ReadyForRemove = false;
	public int FastReloading = 0;
	public int SmokeCount = 0;
	public TThrownWeapon(IObject obj, int id) {
		Id = id;
		if (Id == 2 || Id == 3) {
			Object = GlobalGame.CreateObject("DrinkingGlass00", obj.GetWorldPosition(), obj.GetAngle(), obj.GetLinearVelocity(), obj.GetAngularVelocity());
			ObjectToRemove.Add(Object);
			obj.Remove();
		} else {
			Object = obj;
		}
		Position = Object.GetWorldPosition();
		if (Id == 2 || Id == 3) FastReloading = 100;
		if (Id == 2) {
			SmokeCount = 400;
		} 
	}
	public void OnDestroyed() {
		IsDestroyed = true;	
		if (Id == 1) {
			GlobalGame.SpawnFireNodes(Position, 50, new Vector2(0,0), 3, 10, FireNodeType.Flamethrower);
		}
		if (Id == 0 || Id == 1) ReadyForRemove = true;
		if (Id == 2 || Id == 3) {
			Object = GlobalGame.CreateObject("DrinkingGlass00", Position);
		}
	}
	public void Update() {		
		if (FastReloading > 0) FastReloading--;
		if (Object != null && !Object.RemovalInitiated && !Object.IsRemoved)	Position = Object.GetWorldPosition();
		else if (!IsDestroyed) OnDestroyed();
		if (Id == 2 && FastReloading == 0) {
			if (SmokeCount > 0) {
				SmokeCount--;
				SpawnSmokeCircle(0, 1);	
				SpawnSmokeCircle(20, 4);				
				FastReloading = 2;
				SmokePlayers(30);				
			}
			else ReadyForRemove = true;
		} else if (Id == 3 && FastReloading == 0) {
			StunExplosion(Position + new Vector2(0, 10), 100, 200);
			GlobalGame.PlayEffect("EXP", Position);
			GlobalGame.PlayEffect("S_P", Position);
			GlobalGame.PlayEffect("S_P", Position);
			GlobalGame.PlaySound("Explosion", Position, 1);
			Object.Remove();
			ReadyForRemove = true;
		}
	}
	public bool IsRemove() {
		return ReadyForRemove;
	}
	public void SpawnSmokeCircle(float radius, int nodeCount) {
		float angle = 0;
		float step = (float)Math.PI * 2 / nodeCount;
		while (angle < Math.PI * 2) {
			float y = (float)Math.Sin(angle) * radius;
			float x = (float)Math.Cos(angle) * radius;
			GlobalGame.PlayEffect("STM", Position + new Vector2(x, y));
			angle++;
		} 
	}
	public void SmokePlayers(float radius) {
		for (int i = 0; i < PlayerList.Count; i++) {
			IPlayer pl = PlayerList[i].User.GetPlayer();
			if (pl == null || pl.IsDead) continue;
			if ((pl.GetWorldPosition() - Position).Length() <= radius) {
				PlayerList[i].InSmoke = 3;
			}
		}
	}
}

public static void CreateThrownWeapon(WeaponItem item, int id, Vector2 pos) {
	string name = "";
	if (item == WeaponItem.GRENADES) name = "WpnGrenadesThrown";
	else if (item == WeaponItem.MOLOTOVS) name = "WpnMolotovsThrown";
	else if (item == WeaponItem.MINES) name = "WpnMineThrown";
	else if (item == WeaponItem.SHURIKEN) name = "WpnShurikenThrown";
	IObject[] list = GlobalGame.GetObjectsByName(name);
	IObject obj = null;
	float dist = 20;
	bool have = false;
	for (int i = 0; i < list.Length; i++) {
		float currentDist = (list[i].GetWorldPosition() - pos).Length();
		if (currentDist < dist) {
			have = false;
			for (int j = 0; j < ThrownTrackingList.Count; j++) {
				if (ThrownTrackingList[j].Object == list[i]) {
					have = true;
					break;
				}
			}
			if (have) continue;
			obj = list[i];
			dist = currentDist;
		}
	}	
	if (obj == null) return;
	ThrownTrackingList.Add(new TThrownWeapon(obj, id));	
}

public static void ThrownWeaponUpdate() {
	for (int i = 0; i < ThrownTrackingList.Count; i++) {
		if (ThrownTrackingList[i].IsRemove()) {					
			ThrownTrackingList.RemoveAt(i);
			i--;
		} else {			
			ThrownTrackingList[i].Update();
		}
	}
}

public class TWeapon {
	public IObject Object = null;
	public WeaponItem Weapon = WeaponItem.NONE;
	public Vector2 Position = new Vector2(0, 0);
	public int TotalAmmo = 0;
	public bool DNAProtection = false;
	public PlayerTeam TeamDNA = PlayerTeam.Independent;
	public bool CanOverheat = false;
	public float Overheating = 0;
	public float ShotHeat = 0;
	public float Cooling = 0;
	public int CustomId = 0;
	//methods
	public TWeapon(WeaponItem id) {
		Weapon = id;
		if (Weapon == WeaponItem.MAGNUM) {
			CanOverheat = true;
			ShotHeat = 50;
			Cooling = 0.2f;
		}
	}
	public void OnFire(TPlayer player, WeaponItemType type) {
		if (CanOverheat) {
			Overheating += ShotHeat;
			if (Overheating >= 100) {
				player.User.GetPlayer().RemoveWeaponItemType(type);
				GlobalGame.PlayEffect("CFTXT", player.Position, "OVERHEATED");
				GlobalGame.PlayEffect("EXP", player.Position);
				player.Hp -= 50;
				player.Bleeding = true;
				Vector2 pos = player.Position;
				GlobalGame.CreateObject("MetalDebris00A", pos,(float)rnd.NextDouble());
				pos.X += 5;
				GlobalGame.CreateObject("MetalDebris00B", pos, (float)rnd.NextDouble());
				pos.X -= 10;
				GlobalGame.CreateObject("MetalDebris00C", pos, (float)rnd.NextDouble());

			} else {
				GlobalGame.PlayEffect("CFTXT", player.Position, "OVERHEATING " + ((int)Overheating).ToString() + "%");
			}
		}
		if (DNAProtection && player.Team != TeamDNA) {
			player.User.GetPlayer().RemoveWeaponItemType(type);
			GlobalGame.PlayEffect("CFTXT", player.Position, "DNA SCAN ERROR");
			GlobalGame.TriggerExplosion(player.Position);			
			DNAProtection = false;
		}
		if (type == WeaponItemType.Thrown) {
			CreateThrownWeapon(Weapon, CustomId, player.Position);
		}
	}
	public void Update() {
		if (CanOverheat) {
			if (Overheating > 0) Overheating -= Cooling;
			if (Overheating < 0) Overheating = 0;
		}
	}
	public void Remove() {
		if (Object != null) Object.Remove();
	}
}

public void RemoveWeapons() {
	for (int i = 0; i < WeaponTrackingList.Count; i++) {
		WeaponTrackingList[i].Remove();
	}
	WeaponTrackingList.Clear();
}

public static TWeapon PlayerDropWeaponUpdate(Vector2 pos, WeaponItem id) {
	float dist = 16;
	IPlayer pl = null;
	TPlayer player = null;
	bool hasWeapon = false;
	TWeapon weapon = null;
	float currentDist = 0;
	for (int i = 0; i < PlayerList.Count; i++) {
		currentDist = (pos - PlayerList[i].Position).Length();
		if (currentDist <= dist) {
			hasWeapon = false;
			pl = PlayerList[i].User.GetPlayer();			
			if (PlayerList[i].PrimaryWeapon != null && PlayerList[i].PrimaryWeapon.Weapon == id && (pl != null && pl.CurrentPrimaryWeapon.WeaponItem == WeaponItem.NONE) || pl == null) hasWeapon = true;
			else if (PlayerList[i].SecondaryWeapon != null && PlayerList[i].SecondaryWeapon.Weapon == id && (pl != null && pl.CurrentSecondaryWeapon.WeaponItem == WeaponItem.NONE) || pl == null) hasWeapon = true;
			else if (PlayerList[i].ThrownWeapon != null && PlayerList[i].ThrownWeapon.Weapon == id && (pl != null && pl.CurrentThrownItem.WeaponItem == WeaponItem.NONE) || pl == null) hasWeapon = true;
			if (hasWeapon) {
				player = PlayerList[i];
				dist = currentDist;
			}
		}
	}
	if (player == null) return null;
	if (player.PrimaryWeapon != null && player.PrimaryWeapon.Weapon == id) {
		weapon = player.PrimaryWeapon;
		player.PrimaryWeapon = null;
	} else if (player.SecondaryWeapon != null && player.SecondaryWeapon.Weapon == id) {
		weapon = player.SecondaryWeapon;
		player.SecondaryWeapon = null;
	} else if (player.ThrownWeapon != null && player.ThrownWeapon.Weapon == id) {
		weapon = player.ThrownWeapon;
		player.ThrownWeapon = null;
	}
	return weapon;
}

public static bool IsPlayerDropWeapon(Vector2 pos, WeaponItem id) {
	float dist = 16;
	TWeapon weapon = null;
	float currentDist = 0;
	for (int i = 0; i < WeaponTrackingList.Count; i++) {
		if (!WeaponTrackingList[i].Object.RemovalInitiated) continue;
		currentDist = (pos - WeaponTrackingList[i].Position).Length();
		if (WeaponTrackingList[i].Weapon == id && currentDist <= dist) {
			weapon = WeaponTrackingList[i];
			dist = currentDist;
		}
	}
	return weapon != null;
}

public static void PreWeaponTrackingUpdate() {
	string[] argArray = new string[WeaponItemNames.Count];
	WeaponItemNames.Values.CopyTo(argArray, 0);
	IObject[] list = GlobalGame.GetObjectsByName(argArray);
	IObject obj = null;
	bool found = false;;
	for (int i = 0; i < list.Length; i++) {
		obj = list[i];
		found = false;
		for (int j = 0; j < WeaponTrackingList.Count; j++)  {
			if  (WeaponTrackingList[j].Object == obj) {
				found = true;
				break;
			}
		}
		if (found) continue;
		TWeapon weapon = PlayerDropWeaponUpdate(obj.GetWorldPosition(), ((IObjectWeaponItem)obj).WeaponItem);
		if (weapon == null) {
			weapon = new TWeapon(((IObjectWeaponItem)obj).WeaponItem);
		}
		weapon.Object = obj;
		WeaponTrackingList.Add(weapon);
	}
}

public static TWeapon PlayerPickUpWeaponUpdate(Vector2 pos, WeaponItem id) {
	if (WeaponTrackingList.Count == 0) return null;
	float dist = 16;
	TWeapon weapon = null;
	float currentDist = 0;
	int index = 0;
	for (int i = 0; i < WeaponTrackingList.Count; i++) {
		if (!WeaponTrackingList[i].Object.RemovalInitiated) continue;
		currentDist = (pos - WeaponTrackingList[i].Position).Length();
		if (WeaponTrackingList[i].Weapon == id && currentDist <= dist) {
			weapon = WeaponTrackingList[i];
			dist = currentDist;
			index = i;
		}
	}
	if (weapon == null) return null;
	WeaponTrackingList.RemoveAt(index);
	return weapon;
}

public static void PostWeaponTrackingUpdate() {	
	for (int i = 0; i < WeaponTrackingList.Count; i++) {
		if (WeaponTrackingList[i].Object == null || WeaponTrackingList[i].Object.RemovalInitiated) {
			WeaponTrackingList.RemoveAt(i);
			i--;
		} else {
			WeaponTrackingList[i].Position = WeaponTrackingList[i].Object.GetWorldPosition();
			WeaponTrackingList[i].Update();
		}
	}
}



public class TEffect {
	string Id = "";
	int CurrentTime = 0;
	int Time = 0;
	int Count = 0;
	IObject Object;
	public TEffect(IObject obj, string id, int time, int count) {
		Id = id;
		Object = obj;
		Time = time;
		CurrentTime = time;
		Count = count;
	}
	public bool Update() {
		if (Object == null) return false;
		if (Count <= 0) return false;
		if (CurrentTime <= 0) {
			GlobalGame.PlayEffect(Id, Object.GetWorldPosition());
			CurrentTime = Time;
			Count--;
		}
		CurrentTime--;
		return true;
	}
}

public class TEquipmentSlot {
	public string Name = "";
	public List <TEquipment> EquipmentList = new List <TEquipment>();	
	public TEquipmentSlot(string name) {
		Name = name;
	}
	public void AddEquipment(int id, int cost, int level, string name, string description = "", int accessLevel = 0) {
		TEquipment equipment = new TEquipment(id, cost, level, name, description, accessLevel);
		EquipmentList.Add(equipment);
	}
	public TEquipment Get(int id) {
		for (int i = 0; i < EquipmentList.Count; i++) {
			if (EquipmentList[i].Id == id) return EquipmentList[i];
		}
		return null;
	}
}

public void AddLevel(string name, int needExp, int allowPoints) {
	TLevel level = new TLevel(name, needExp, allowPoints);
	LevelList.Add(level);
}

public class TStrikeInfo {
	public int Id = 0;
	public int Angle = 0;
}

public class TPlayerStrikeInfo {
	public IPlayer Player;
	public List <TStrikeInfo> StrikeList = new List <TStrikeInfo>();
}

public class TCapturePoint {
	public IObjectText Object;
	public int DefaultCaptureProgress = 0;	
	public int CaptureProgress = 0;
	public TCapturePoint(IObjectText obj, int captureProgress) {
		Object = obj;
		DefaultCaptureProgress = captureProgress;
	}
	public void Start() {
		CaptureProgress = DefaultCaptureProgress;
	}
	public void Update() {
		for (int i = 0; i < PlayerList.Count; i++) {			
			IPlayer pl = PlayerList[i].User.GetPlayer();
			if (pl != null && !pl.IsDead) {
				if (TestDistance(pl.GetWorldPosition(), Object.GetWorldPosition(), CapturePointRadius)) {
					if (pl.GetTeam() == PlayerTeam.Team1) {
						if (CaptureProgress < MaxCapturePointProgress) {
							CaptureProgress++;
							PlayerList[i].AddExp(0.05f, 2);
						}
					} else {
						if (CaptureProgress > -MaxCapturePointProgress) {
							CaptureProgress--;
							PlayerList[i].AddExp(0.05f, 2);
						}
					}
				}
			}
		}
		float percent = ((float)Math.Abs(CaptureProgress)) / ((float)MaxCapturePointProgress);
		byte color = (byte)(255f * percent);
		if (CaptureProgress >=0) {
			Object.SetTextColor(new Color((byte)(255 - color), (byte)(255 - color), 255));
		} else {
			Object.SetTextColor(new Color(255, (byte)(255 - color), (byte)(255 - color)));
		}
	}
}

public class TMapPart {
	public List <TCapturePoint> PointList = new List <TCapturePoint>();
	public List <IObject> RedSpawnPosition = new List <IObject>();
	public List <IObject> BlueSpawnPosition = new List <IObject>();
	public Vector2 MapPosition = new Vector2(0, 0);
	public int CapturedBy = 0;
	//functions
	public void Start() {
		CapturedBy = 0;
		CameraPosition = MapPosition;
		int blue = 0, red = 0;
		for (int i = 0; i < PlayerList.Count; i++) {
			if (PlayerList[i].Team == PlayerTeam.Team1) {				
				PlayerMenuList[i].SpawnPlayer(BlueSpawnPosition[blue].GetWorldPosition());
				blue++;
			} else {
				PlayerMenuList[i].SpawnPlayer(RedSpawnPosition[red].GetWorldPosition());
				red++;
			}			
		}	
		for (int i = 0; i < PointList.Count; i++) {
			PointList[i].Start();
		}
		
		if (GlobalRandom.Next(0, 100) <= 20) {
			WeatherType weather = GlobalGame.GetWeatherType();
			if (weather == WeatherType.None) GlobalGame.SetWeatherType(WeatherType.Rain);
			else GlobalGame.SetWeatherType(WeatherType.None);
		}
	}
	public int Update() {
		if (CapturedBy != 0) return 0;
		bool blueWin = true;
		bool redWin = true;
		for (int i = 0; i < PointList.Count; i++) {
			PointList[i].Update();
			if (PointList[i].CaptureProgress != MaxCapturePointProgress) {
				blueWin = false;
			}
			if (PointList[i].CaptureProgress != -MaxCapturePointProgress) {
				redWin = false;
			}
		}
		if (blueWin) {
			CapturedBy = 1;
			return 1;
		} else if (redWin){
			CapturedBy = 2;
			return 2;
		}		
		return 0;
	}
}

public class TPlayerMenu {
	public IObjectText Menu;
	public TPlayer Player = null;
	public int CurrentPoints = 0;
	public List <int> Equipment = new List <int>();
	public int CurrentGroup = 0;
	public int ActionTimer = 0;
	public bool Ready = false;
	public bool Change = true;
	public bool IsDescription = false;
	public int AccessLevel = 0;
	public void SetPlayer(TPlayer player) {
		if (Player != null) return;
		Player = player;
		if (UserAccessList.ContainsKey(player.User.Name)) AccessLevel = UserAccessList[player.User.Name];
		for (int i = 0; i < EquipmentList.Count; i++) Equipment.Add(0);		
		Equipment[0] = 1;
		Equipment[2] = 3;
	}
	public string Save() {
		if (Player.Name.Contains("Unnamed")) return "";
		string data = Player.Save();
		for (int i = 0; i < Equipment.Count; i++) {
			data += Equipment[i].ToString();
			if (i == Equipment.Count - 1) {
				data += ";";
			} else {
				data += ":";
			}
		}
		return data;
	}
	public void ValidateEquipment() {
		for (int i = 0; i < Equipment.Count; i++) {
			if (EquipmentList[i].EquipmentList.Count <= Equipment[i]) Equipment[i] = 0;
		}
	}
	public void ShowExp() {	
		string text = "";		
		text += Player.Name + "\n";
		//text += "Class: " + race.Name + "\n";
		text += "Level " + (Player.Level + 1).ToString() + " \"" + LevelList[Player.Level].Name + "\"";
		if (AccessLevel == 1) text += " [$]";
		text += "\n";
		if (Player.Level  + 1 <LevelList.Count) {
			int percent = (int)((float)Player.CurrentExp / (float)LevelList[Player.Level + 1].NeedExp * 10);
			text += "[";
			for (int i = 0; i < 10; i++) {
				if (i < percent) {
					text += "=";
				} else {
					text += "_";
				}
			}
			text += "] " + ((int)Player.CurrentExp) + "/" + LevelList[Player.Level + 1].NeedExp + "\n";
		}
		text += "------------------------------\n";
		if (Player.ExpSource[0] > 0) text += "Kills: +" + ((int)Player.ExpSource[0]).ToString() + "\n";
		if (Player.ExpSource[5] > 0) text += "Reinforcements: +" + ((int)Player.ExpSource[5]).ToString() + "\n";
		if (Player.ExpSource[1] > 0) text += "Healing: +" + ((int)Player.ExpSource[1]).ToString() + "\n";
		if (Player.ExpSource[2] > 0) text += "Point Capture: +" + ((int)Player.ExpSource[2]).ToString() + "\n";
		if (Player.ExpSource[3] > 0) text += "Area Capture: +" + ((int)Player.ExpSource[3]).ToString() + "\n";
		if (Player.ExpSource[4] > 0) text += "Win: +" + ((int)Player.ExpSource[4]).ToString() + "\n";
		if (Player.IsNewLevel) {
			text += "---------New equipment--------\n";
			int lineLength = 30;
			int lineLeft = lineLength;
			for (int i = 0; i < EquipmentList.Count; i++) {
				for (int j = 0; j < EquipmentList[i].EquipmentList.Count; j++) {
					if (EquipmentList[i].EquipmentList[j].Level == Player.Level && EquipmentList[i].EquipmentList[j].AccessLevel <= AccessLevel) {
						string name = EquipmentList[i].EquipmentList[j].Name;
						if (name.Length + 2 > lineLeft) {
							text += "\n";
							lineLeft = lineLength;
						}
						lineLeft -= name.Length;
						text += name + ", ";
					}
				}
			}
		}
		Menu.SetTextColor(Color.White);
		Menu.SetText(text);
	}
	public void Update() {
		IPlayer pl = Player.User.GetPlayer();
		Player.UpdateActiveStatus();
		if (pl == null) return;		
		if (!pl.IsDead && !Ready) {
			if (ActionTimer <= 0) {
				if (IsDescription) {
					if (pl.IsBlocking) {
						if (EquipmentList[CurrentGroup].EquipmentList[Equipment[CurrentGroup]].Description != "") {
							IsDescription = ! IsDescription;
							Change = true;
							ActionTimer = 25;
						}
					}
				} else {
					if (pl.IsInMidAir) {
						CurrentGroup = Math.Max(CurrentGroup - 1, 0);
						ActionTimer = 25;
						Change = true;
					} else if (pl.IsMeleeAttacking && CurrentPoints <= LevelList[Player.Level].AllowPoints) {
						Ready = true;
						CurrentGroup = -1;
						Change = true;
					} else if (pl.IsCrouching) {
						CurrentGroup = Math.Min(CurrentGroup + 1, Equipment.Count - 1);	
						ActionTimer = 15;		
						Change = true;		
					} else if (pl.IsRunning) {
						if (pl.FacingDirection == 1) {
							for (int i = Equipment[CurrentGroup] + 1; i < EquipmentList[CurrentGroup].EquipmentList.Count; i++) {
								if (EquipmentList[CurrentGroup].EquipmentList[i].Level <= Player.Level && EquipmentList[CurrentGroup].EquipmentList[i].AccessLevel <= AccessLevel) {
									Equipment[CurrentGroup] = i;
									Change = true;
									break;
								}	
							}
						} else {
							for (int i = Equipment[CurrentGroup] - 1; i >= 0; i--) {
								if (EquipmentList[CurrentGroup].EquipmentList[i].Level <= Player.Level && EquipmentList[CurrentGroup].EquipmentList[i].AccessLevel <= AccessLevel) {
									Equipment[CurrentGroup] = i;
									Change = true;
									break;
								}	
							}
						}
						ActionTimer = 15;
					} else if (pl.IsBlocking) {
						if (EquipmentList[CurrentGroup].EquipmentList[Equipment[CurrentGroup]].Description != "") {
							IsDescription = ! IsDescription;
							Change = true;
							ActionTimer = 25;							
						}
					}
				}			
			} else {
				ActionTimer--;
			}
		}
		if (Change) {
			int lineCount = 14;
			string text = "";
			text += Player.Name + "\n";
			CurrentPoints = 0;
			for (int i = 0; i < Equipment.Count; i++) {
				CurrentPoints += EquipmentList[i].EquipmentList[Equipment[i]].Cost;
			}		
			if (LevelList.Count > 0) {
				text += "Level " + (Player.Level + 1).ToString() + " \"" + LevelList[Player.Level].Name + "\"";		
				if (AccessLevel == 1) text += " [$]";
				text += "\n";
				if (Player.Level  + 1 < LevelList.Count) {
					int percent = (int)((float)Player.CurrentExp / (float)LevelList[Player.Level + 1].NeedExp * 10);
					text += "[";
					for (int i = 0; i < 10; i++) {
						if (i < percent) {
							text += "=";
						} else {
							text += "_";
						}
					}
					text += "] " + Player.CurrentExp + "/" + LevelList[Player.Level + 1].NeedExp + "\n";
				}
				if (Ready) {
					Menu.SetTextColor(Color.Green);						
					text += "READY TO BATTLE\n";
				} else {
					text += "Equipment: " + CurrentPoints.ToString() + "/" + LevelList[Player.Level].AllowPoints + "\n\n";
					if (IsDescription) {
						int maxLength = 30;
						string[] words = (EquipmentList[CurrentGroup].EquipmentList[Equipment[CurrentGroup]].Name + ": " + EquipmentList[CurrentGroup].EquipmentList[Equipment[CurrentGroup]].Description).Split(' ');
						int currentLen = 0;
						for (int i = 0; i < words.Length; i++) {
							if (currentLen + words[i].Length > maxLength) {
								text += "\n";
								currentLen = 0;
							}
							text += words[i] + " ";
							currentLen += words[i].Length + 1;
						}
					} else {
						for (int i = 0; i < Equipment.Count; i++) {
							if (EquipmentList[i].EquipmentList.Count <= Equipment[i]) Equipment[i] = 0;
							text += ((CurrentGroup == i) ? ">": "") + EquipmentList[i].Name + ": ";
							switch (EquipmentList[i].EquipmentList[Equipment[i]].AccessLevel) {
								case 1: text += "[$]"; break;
								case 2: text += "[TEST]"; break;
							}
							text += EquipmentList[i].EquipmentList[Equipment[i]].Name + "[" + EquipmentList[i].EquipmentList[Equipment[i]].Cost.ToString() + "]\n";
						}			
						if (CurrentPoints > LevelList[Player.Level].AllowPoints) {
							Menu.SetTextColor(Color.Red);
						}  else {
							Menu.SetTextColor(Color.White);
						}
					}
				}
			} else {
				Menu.SetTextColor(Color.Green);
			}
			
			int additionLines = lineCount - text.Split('\n').Length;
			for (int i = 0; i < additionLines; i++) {
				text += "\n";
			}
			Menu.SetText(text);			
		}
	}
	public void SpawnPlayer(Vector2 position) {			
		if (!Player.IsActive()) return;
		if (!Ready && CurrentPoints <= LevelList[Player.Level].AllowPoints) Ready = true;
		Player.Respawn();
		if (Player.User.GetPlayer() != null) Player.User.GetPlayer().Remove();
		IPlayer newPlayer = GlobalGame.CreatePlayer(position);			
		newPlayer.SetUser(Player.User);
		newPlayer.SetTeam(Player.Team);
		//newPlayer.SetStatusBarsVisible(false);
		if (Ready) {
			for (int i = 0; i < Equipment.Count; i++) {
				Player.AddEquipment(EquipmentList[i].EquipmentList[Equipment[i]].Id, i);
			}
		}
		newPlayer.SetProfile(Player.GetSkin());
		Player.OnPlayerCreated();
		newPlayer.SetInputEnabled(false);
	}
}

public class TShieldGenerator {
	public class TShieldBlock {
		public Vector2 Position;
		public float Angle = 0;
		public List<IObject> ShieldGlass = new List<IObject>();
		public IObject ShieldBlock = null;
		public List<IObject> ShieldShard = new List<IObject>();
		public PlayerTeam Team;
		public float Sin;
		public float Cos;
		public float NSin;
		public float NCos;
		public int CheckShieldTimer = 0;
		public bool Disabled = false;
		public int NeedPower = 0;
		public int AnimationTime = 0;
		public TShieldBlock(Vector2 position, float angle, PlayerTeam team) {
			Position = position;
			Angle = angle;
			Team = team;
			Sin = (float)Math.Sin(Angle);
			Cos = (float)Math.Cos(Angle);
			NSin = (float)Math.Sin(Angle + (float)Math.PI / 2);
			NCos = (float)Math.Cos(Angle + (float)Math.PI / 2);
			for (int i = 0; i < 2; i++) {
				ShieldGlass.Add(null);
				ShieldShard.Add(null);
			}
			AnimationTime = GlobalRandom.Next(50, 150);
		}
		public void Update() {
			NeedPower = 0;
			if (CheckShieldTimer == 0) {
				bool d = false;
				for (int i = 0; i < PlayerList.Count; i++) {
					if (PlayerList[i].Team == Team) {
						IPlayer pl = PlayerList[i].User.GetPlayer();
						if (pl == null) continue;
						if (TestDistance(Position, PlayerList[i].Position, 28)) {
							d = true;
							break;
						}
					}  
				}				
				if (d && !Disabled) {
					Remove();
					NeedPower = -2;					
				}
				Disabled = d;
				CheckShieldTimer = 30;
			} else if (CheckShieldTimer > 0) {
				CheckShieldTimer--;
			}
			if (Disabled) return;			
			if (ShieldBlock == null || ShieldBlock.IsRemoved) {
				ShieldBlock = GlobalGame.CreateObject("InvisibleBlock", Position + new Vector2(NCos * 4, NSin * 4), Angle);
				ShieldBlock.SetSizeFactor(new Point(1, 2));
			}
			for (int i = 0; i < ShieldGlass.Count; i++) {
				if (ShieldGlass[i] == null) {
					int s = (int)Math.Pow(-1, i);
					ShieldGlass[i] = GlobalGame.CreateObject("ReinforcedGlass00A", Position + new Vector2(NCos * 4 * s, NSin * 4 * s) - new Vector2(Cos, Sin), Angle);
				} else if (ShieldGlass[i].IsRemoved) {
					ShieldGlass[i].Remove();
					ShieldGlass[i] = null;
				}
			}
			for (int i = 0; i < ShieldShard.Count; i++) {
				int s = (int)Math.Pow(-1, i);
				if (ShieldShard[i] == null) {
					ShieldShard[i] = GlobalGame.CreateObject("GlassShard00A", Position + new Vector2(NCos * 4 * s, NSin * 4 * s) + new Vector2(Cos * 4, Sin * 4), Angle - (float)Math.PI / 2);
					ShieldShard[i].SetBodyType(BodyType.Static);					
				} else if (ShieldShard[i].IsRemoved) {
					ShieldShard[i].Remove();
					ShieldShard[i] = null;
					NeedPower++;
				}
			}
			if (NeedPower > 0) {
				int offset = GlobalRandom.Next(-8, 8);
				GlobalGame.PlayEffect("S_P", Position + new Vector2(Cos * offset, Sin * offset));
			}
			if (AnimationTime <= 0) {
				AnimationTime = GlobalRandom.Next(50, 150);
				int offset = GlobalRandom.Next(-8, 8);
				GlobalGame.PlayEffect("GLM", Position + new Vector2(Cos * offset, Sin * offset));
			} else if (AnimationTime > 0) {
				AnimationTime--;
			}
		}
		public void Remove() {
			if (ShieldBlock != null) ShieldBlock.Remove();
			for (int i = 0; i < ShieldGlass.Count; i++) {
				if (ShieldGlass[i] != null) ShieldGlass[i].Remove();
			}
			for (int i = 0; i < ShieldShard.Count; i++) {
				if (ShieldShard[i] != null) ShieldShard[i].Remove();
			}
		}
		public void Destroy() {
			if (ShieldBlock != null) ShieldBlock.Remove();
			for (int i = 0; i < ShieldGlass.Count; i++) {
				if (ShieldGlass[i] != null) ShieldGlass[i].Destroy();
			}
			for (int i = 0; i < ShieldShard.Count; i++) {
				if (ShieldShard[i] != null) ShieldShard[i].Destroy();
			}
		}
	}
	public IObject CoreObject;
	public IObjectText TextName;
	public List<IObject> OtherObjects = new List<IObject>();
	public List<TShieldBlock> ShieldBlocks = new List<TShieldBlock>();
	public PlayerTeam Team;
	public float Power = 0;
	public float Radius = 0;
	public bool IsEnabled = false;
	public int Loading = 50;
	public TShieldGenerator(int power, Vector2 position, float radius, PlayerTeam team) {
		Team = team;
		Power = power;
		Radius = radius;
		CoreObject = GlobalGame.CreateObject("Computer00", position, 0);
		TextName = (IObjectText)GlobalGame.CreateObject("Text", position);
		TextName.SetTextAlignment(TextAlignment.Middle);
		TextName.SetTextScale(0.8f);
		IObject leftLeg = GlobalGame.CreateObject("Duct00C_D", position + new Vector2(-5, -2), (float)Math.PI / 2);
		IObject rightLeg = GlobalGame.CreateObject("Duct00C_D", position + new Vector2(6, -2), (float)Math.PI / 2);
		IObjectWeldJoint joint = (IObjectWeldJoint)GlobalGame.CreateObject("WeldJoint", position);
		joint.AddTargetObject(CoreObject);
		joint.AddTargetObject(leftLeg);
		joint.AddTargetObject(rightLeg);
		OtherObjects.Add(leftLeg);
		OtherObjects.Add(rightLeg);
		OtherObjects.Add(joint);
		OtherObjects.Add(TextName);
	}
	public void CreateShield() {
		float angleStep = (float)Math.Acos((double)(1f - (16f * 16f) / (2f * Radius * Radius)));
		int count = (int)Math.Round(Math.PI * 2 / angleStep);
		angleStep = (float)(Math.PI * 2 / count);
		float currentAngle = 0;
		for (int i = 0; i < count; i++) {
			Vector2 position = CoreObject.GetWorldPosition() + new Vector2((float)Math.Cos(currentAngle) * Radius, (float)Math.Sin(currentAngle) * Radius);
			int status = TracePath(CoreObject.GetWorldPosition(), position, PlayerTeam.Independent, true);
			if (status < 3) {
				TShieldBlock block = new TShieldBlock(position, currentAngle, Team);
				ShieldBlocks.Add(block);
			}
			currentAngle += angleStep;
		}		
	}
	public void DestroyShield() {
		for (int i = 0; i < ShieldBlocks.Count; i++) {
			ShieldBlocks[i].Destroy();
		}
		ShieldBlocks.Clear();
	}
	public void RemoveShield() {
		for (int i = 0; i < ShieldBlocks.Count; i++) {
			ShieldBlocks[i].Remove();
		}
		ShieldBlocks.Clear();
	}
	public void Update() {
		if (Loading > 0) {
			Loading--;
			return;
		}
		if (CoreObject == null || CoreObject.IsRemoved) {
			if (OtherObjects.Count > 0) {
				for (int i = 0; i < OtherObjects.Count; i++) {
					if (OtherObjects[i] != null) OtherObjects[i].Destroy();
				}
				OtherObjects.Clear();
			}
			return;
		}
		TextName.SetWorldPosition(CoreObject.GetWorldPosition() + new Vector2(0, 10));
		string name = "Shield Generator" + "[" + Power + "]";
		TextName.SetText(name);
		if (Team == PlayerTeam.Team1) TextName.SetTextColor(new Color(122, 122, 224));
		else if (Team == PlayerTeam.Team2) TextName.SetTextColor(new Color(224, 122, 122));
		else if (Team == PlayerTeam.Team3) TextName.SetTextColor(new Color(112, 224, 122));
		if (Power > 0) {
			float velocity = CoreObject.GetLinearVelocity().Length();
			if (!IsEnabled && velocity == 0) {
				IsEnabled = true;
				CreateShield();
			} else if (IsEnabled && velocity != 0) {
				IsEnabled = false;
				DestroyShield();
			}
			for (int i = 0; i < ShieldBlocks.Count; i++) {
				ShieldBlocks[i].Update();
				Power -= ShieldBlocks[i].NeedPower;
			}
			Power = Math.Max(0, Power);
			if (Power <= 0) DestroyShield();
		}
	}
	public void Remove() {
		CoreObject.Remove();
		for (int i = 0; i < OtherObjects.Count; i++) {
			OtherObjects[i].Remove();
		}
		RemoveShield();
	}
}

public static void CreateShieldGenerator(int power, Vector2 position, float radius, PlayerTeam team) {
	TShieldGenerator generator = new TShieldGenerator(power, position, radius, team);
	ShieldGeneratorList.Add(generator);
}

public class TTurret {
	public class TTurretWeapon {
		public int BulletType;
		public string Sound;
		public int ReloadingTime;
		public int CurrentReloading = 100;
		public int Ammo;
		public int Scatter;
		public bool SuppressiveFire = false;
		public int MaxFireDelay = 0;
		public int MaxBulletCount = 0;
		public int FireDelay = 1;
		public int BulletCount = 0;
		public int Distance = 2000;
		public bool TurretTarget = true;
	}
	public int Id;
	public PlayerTeam Team;
	public List <TTurretWeapon> WeaponList = new List <TTurretWeapon>();
	public List <IObject> OtherObjects = new List <IObject>();
	public List <IObject> DamagedObjects = new List <IObject>();
	public List <float> DamagedObjectHp = new List <float>();
	public List <float> DamagedObjectMaxHp = new List <float>();
	public IObject Hull;
	public IObject MainBlock;
	public IObjectRevoluteJoint MainMotor;
	public IObjectText TextName;
	public IObjectRailJoint RailJoint = null;
	public IObjectTargetObjectJoint TargetObject = null;
	public IObjectRailAttachmentJoint RailAttachment = null;
	public string Name;
	public float RotationSpeed = 1f;
	public float RotationLimit = (float)Math.PI / 3;
	public float MainBlockAngle;
	public float DefaultAngle;	
	public float DamageFactor = 3f;	
	public IObject Target = null;
	public int TargetVision = 3;
	public int LastTargetFinding = 10;
	public int LastPathFinding = 0;
	public bool EnableMovement = false;
	public int PathSize = 0;
	public float Speed = 0;
	public List <Vector2> CurrentPath = new List <Vector2>();
	public bool HackingProtection = false;
	public bool IsProtector = false;
	public bool IsCapturer = false;
	public float DroneMinDistance = 0;
	public int SmokeEffectTime = 0;
	public TTurret(int id, Vector2 position, int dir, PlayerTeam team) {
		Id = id;
		Team = team;	
		if (dir < 0) {
			MainBlockAngle = (float)Math.PI / 2;
			DefaultAngle = (float)Math.PI;
		} else {
			MainBlockAngle = (float)Math.PI / 2;
			DefaultAngle = 0;
		}
		IObject leftLeg = null;
		IObject rightLeg = null;
		IObject hull2 = null;
		IObject hull3 = null;
		IObject hull4 = null;

		if (Id < 4) {
			leftLeg = GlobalGame.CreateObject("Duct00C_D", position + new Vector2(-3, -9), 1.2f);
			rightLeg = GlobalGame.CreateObject("Duct00C_D", position + new Vector2(3, -9), -1.2f);
			leftLeg.SetMass(leftLeg.GetMass() * 20);
			rightLeg.SetMass(rightLeg.GetMass() * 20);
		} else if (Id == 4 || Id == 5) {
			RotationLimit = 0;
			Hull = GlobalGame.CreateObject("BgMetal08H", position + new Vector2(-4, 4), (float)Math.PI / 2);
			hull2 = GlobalGame.CreateObject("BgMetal08H", position + new Vector2(4, 4), 0);
			hull3 = GlobalGame.CreateObject("BgMetal08H", position + new Vector2(4, -4), -(float)Math.PI / 2);
			hull4 = GlobalGame.CreateObject("BgMetal08H", position + new Vector2(-4, -4), -(float)Math.PI);
			hull2.SetBodyType(BodyType.Dynamic);
			hull3.SetBodyType(BodyType.Dynamic);
			hull4.SetBodyType(BodyType.Dynamic);	
		} else if (Id == 6) {
			RotationLimit = 0;
			Hull = GlobalGame.CreateObject("InvisibleBlockNoCollision", position);
		}
		
		if (Id == 6) MainBlock = GlobalGame.CreateObject("CrabCan00", position, - (float)Math.PI / 2 * dir);
		else MainBlock = GlobalGame.CreateObject("Computer00", position, - (float)Math.PI / 2 * dir);
		if (Id >= 4) {
			IObjectAlterCollisionTile collisionDisabler = (IObjectAlterCollisionTile)GlobalGame.CreateObject("AlterCollisionTile", position);
			collisionDisabler.SetDisableCollisionTargetObjects(true);
			collisionDisabler.AddTargetObject(MainBlock);
			IObject []platforms = GlobalGame.GetObjectsByName(new string[] {"MetalPlat01A", "Lift00C", "Lift00B", "MetalPlat00G", "Elevator02B"});
			for (int i = 0; i < platforms.Length; ++i) {
				collisionDisabler.AddTargetObject(platforms[i]);
			}
		}
		IObject antenna = GlobalGame.CreateObject("BgAntenna00B", position + new Vector2(-2 * dir,9));
		antenna.SetBodyType(BodyType.Dynamic);
		antenna.SetMass(0.0000001f);
		IObjectWeldJoint bodyJoint = (IObjectWeldJoint)GlobalGame.CreateObject("WeldJoint", position);
		IObjectWeldJoint hullJoint = (IObjectWeldJoint)GlobalGame.CreateObject("WeldJoint", position);
		if (Id < 4) {
			hullJoint.AddTargetObject(leftLeg);
			hullJoint.AddTargetObject(rightLeg);
			OtherObjects.Add(leftLeg);
			OtherObjects.Add(rightLeg);
		} else if (Id == 4 || Id == 5) {
			hullJoint.AddTargetObject(Hull);
			hullJoint.AddTargetObject(hull2);
			hullJoint.AddTargetObject(hull3);
			hullJoint.AddTargetObject(hull4);
			OtherObjects.Add(Hull);
			OtherObjects.Add(hull2);
			OtherObjects.Add(hull3);
			OtherObjects.Add(hull4);
		} else if (Id == 6) {
			hullJoint.AddTargetObject(Hull);
			OtherObjects.Add(Hull);
		}
		bodyJoint.AddTargetObject(MainBlock);		
		bodyJoint.AddTargetObject(antenna);
		bodyJoint.SetPlatformCollision(WeldJointPlatformCollision.PerObject);
		OtherObjects.Add(antenna);
		OtherObjects.Add(bodyJoint);
		OtherObjects.Add(hullJoint);
		DamagedObjects.Add(MainBlock);
		if (Id == 4 || Id == 5) {
			HackingProtection = true;
			EnableMovement = true;
			PathSize = 2;
			Speed = 3;
			RotationSpeed = 3f;
			DamageFactor = 0.2f;
			DroneMinDistance = 5 * 8;
		} else if (Id == 6) {
			HackingProtection = true;
			EnableMovement = true;
			PathSize = 1;
			Speed = 5;
			RotationSpeed = 3f;
			DamageFactor = 0.2f;
		}

		if (id == 0) Name = "Light Turret";
		else if (id == 1) Name = "Rocket Turret";
		else if (id == 2) Name = "Heavy Turret";
		else if (id == 3) Name = "Sniper Turret";
		else if (id == 4) Name = "Drone";
		else if (id == 5) Name = "Assault Drone";
		else if (id == 6) Name = "Melee Drone";
		if (id == 1 || id == 2) { //@0:5B=8F0
			IObject gun2 = GlobalGame.CreateObject("BgBarberPole00", position + new Vector2(dir, -2), (float)Math.PI / 2);
			IObject gun1 = GlobalGame.CreateObject("BgBarberPole00", position + new Vector2(dir, 3), (float)Math.PI / 2);
			gun1.SetBodyType(BodyType.Dynamic);
			gun2.SetBodyType(BodyType.Dynamic);
			gun1.SetMass(0.0000001f);
			gun2.SetMass(0.0000001f);
			bodyJoint.AddTargetObject(gun1);
			bodyJoint.AddTargetObject(gun2);
			OtherObjects.Add(gun1);
			OtherObjects.Add(gun2);
			TTurretWeapon weapon = new TTurretWeapon();
			weapon.BulletType = (int)ProjectileItem.BAZOOKA;
			weapon.Sound = "Bazooka";
			weapon.ReloadingTime = 200;
			if (id == 2) weapon.Ammo = 4;
			else { 
				weapon.Ammo = 6;
				weapon.SuppressiveFire = true;
				weapon.MaxFireDelay = 1;
				weapon.MaxBulletCount = 1;
			}
			weapon.Scatter = 0;
			WeaponList.Add(weapon);
		}
		if (id == 0 || id == 2 || id == 4) { //?C;5<5B		
			IObject gun00 = GlobalGame.CreateObject("BgPipe02A", position + new Vector2(8 * dir, 0));
			IObject gun01 = GlobalGame.CreateObject("BgPipe02B", position + new Vector2(14 * dir, 0));
			IObject gun10 = GlobalGame.CreateObject("BgPipe02A", position + new Vector2(8 * dir, 2));
			IObject gun11 = GlobalGame.CreateObject("BgPipe02B", position + new Vector2(14 * dir, 2));
			gun00.SetBodyType(BodyType.Dynamic);
			gun01.SetBodyType(BodyType.Dynamic);
			gun10.SetBodyType(BodyType.Dynamic);
			gun11.SetBodyType(BodyType.Dynamic);
			gun00.SetMass(0.0000001f);
			gun01.SetMass(0.0000001f);
			gun10.SetMass(0.0000001f);
			gun11.SetMass(0.0000001f);
			bodyJoint.AddTargetObject(gun00);
			bodyJoint.AddTargetObject(gun01);
			bodyJoint.AddTargetObject(gun10);
			bodyJoint.AddTargetObject(gun11);
			OtherObjects.Add(gun00);
			OtherObjects.Add(gun01);
			OtherObjects.Add(gun10);
			OtherObjects.Add(gun11);
			TTurretWeapon weapon = new TTurretWeapon();
			weapon.BulletType = (int)ProjectileItem.UZI;
			weapon.Sound = "AssaultRifle";
			weapon.ReloadingTime = 7;
			weapon.Ammo = 150;
			weapon.Scatter = 2;
			weapon.SuppressiveFire = true;
			weapon.MaxFireDelay = 30;
			weapon.MaxBulletCount = 3;
			WeaponList.Add(weapon);
		}	
		if (id == 3) { //A=09?5@:0
			IObject gun1 = GlobalGame.CreateObject("BgPipe02A", position + new Vector2((dir == -1) ? -14 : 6, 0));
			IObject gun2 = GlobalGame.CreateObject("BgPipe02B", position + new Vector2(22 * dir, 0));
			IObject gun3 = GlobalGame.CreateObject("BgPipe02B", position + new Vector2(7 * dir, 2));
			gun1.SetSizeFactor(new Point(2, 1));
			gun1.SetBodyType(BodyType.Dynamic);
			gun2.SetBodyType(BodyType.Dynamic);
			gun3.SetBodyType(BodyType.Dynamic);
			gun1.SetMass(0.0000001f);
			gun2.SetMass(0.0000001f);
			gun3.SetMass(0.0000001f);
			bodyJoint.AddTargetObject(gun1);
			bodyJoint.AddTargetObject(gun2);
			bodyJoint.AddTargetObject(gun3);
			OtherObjects.Add(gun1);
			OtherObjects.Add(gun2);
			OtherObjects.Add(gun3);
			TTurretWeapon weapon = new TTurretWeapon();
			weapon.BulletType = (int)ProjectileItem.SNIPER;
			weapon.Sound = "Sniper";
			weapon.ReloadingTime = 150;
			weapon.Ammo = 10;
			weapon.Scatter = 0;
			WeaponList.Add(weapon);
		}
		if (id == 5) { //>3=5<QB
			IObject gun1 = GlobalGame.CreateObject("BgPipe02A", position + new Vector2((dir == -1) ? -14 : 6, 0));
			IObject gun2 = GlobalGame.CreateObject("BgPipe02B", position + new Vector2(14 * dir, 0));
			IObject gun3 = GlobalGame.CreateObject("BgPipe02B", position + new Vector2(7 * dir, 2));
			IObject gun4 = GlobalGame.CreateObject("BgPipe02B", position + new Vector2(7 * dir, -2));
			gun1.SetBodyType(BodyType.Dynamic);
			gun2.SetBodyType(BodyType.Dynamic);
			gun3.SetBodyType(BodyType.Dynamic);
			gun4.SetBodyType(BodyType.Dynamic);
			gun1.SetMass(0.0000001f);
			gun2.SetMass(0.0000001f);
			gun3.SetMass(0.0000001f);
			gun4.SetMass(0.0000001f);
			bodyJoint.AddTargetObject(gun1);
			bodyJoint.AddTargetObject(gun2);
			bodyJoint.AddTargetObject(gun3);
			bodyJoint.AddTargetObject(gun4);
			OtherObjects.Add(gun1);
			OtherObjects.Add(gun2);
			OtherObjects.Add(gun3);
			OtherObjects.Add(gun4);
			TTurretWeapon weapon = new TTurretWeapon();
			weapon.Distance = 150;
			weapon.BulletType = -1;
			weapon.Sound = "Flamethrower";
			weapon.ReloadingTime = 3;
			weapon.Ammo = 300;
			weapon.SuppressiveFire = true;
			weapon.Scatter = 10;
			weapon.TurretTarget = false;
			WeaponList.Add(weapon);
		}
		if (Id == 5 || Id == 6) { //2>;=0 <>;=89 
			TTurretWeapon weapon = new TTurretWeapon();
			weapon.Distance = 50;
			weapon.BulletType = -2;
			weapon.Sound = "Splash";
			weapon.ReloadingTime = 150;
			weapon.SuppressiveFire = true;
			weapon.Ammo = 10;
			weapon.TurretTarget = false;
			WeaponList.Add(weapon);
		}
		TextName = (IObjectText)GlobalGame.CreateObject("Text", position);
		TextName.SetTextAlignment(TextAlignment.Middle);
		TextName.SetText(Name);
		TextName.SetTextScale(0.8f);
		OtherObjects.Add(TextName);
		MainMotor = (IObjectRevoluteJoint)GlobalGame.CreateObject("RevoluteJoint", position);
		MainMotor.SetTargetObjectB(MainBlock);
		if (Id < 4) MainMotor.SetTargetObjectA(leftLeg);
		else MainMotor.SetTargetObjectA(hull2);
		MainMotor.SetMotorEnabled(true);
		MainMotor.SetMaxMotorTorque(100000);
		MainMotor.SetMotorSpeed(0);
		OtherObjects.Add(MainMotor);
		InitHealth();		
	}
	public void InitHealth() {
		for (int i = 0; i < DamagedObjects.Count; i++) {
			DamagedObjectHp.Add(DamagedObjects[i].GetHealth());
			DamagedObjectMaxHp.Add(DamagedObjects[i].GetHealth());
		}
	}
	public bool HaveAmmo() {
		for (int i = 0; i < WeaponList.Count; i++) {
			if (WeaponList[i].Ammo > 0) {
				return true;
			}
		}
		return false;
	}
	public bool CanHitTurret() {
		for (int i = 0; i < WeaponList.Count; i++) if (WeaponList[i].TurretTarget) return true;
		return false;
	}
	public int GetMaxDistance() {
		int max = 0;
		for (int i = 0; i < WeaponList.Count; i++) {
			if (WeaponList[i].Ammo > 0 && WeaponList[i].Distance > max) {
				max = WeaponList[i].Distance;
			}
		}
		return max;
	}
	public void Update() {
		if (SmokeEffectTime > 0) SmokeEffectTime--;
		if (MainBlock.GetHealth() <= 5 && SmokeEffectTime == 0) {
			GlobalGame.PlayEffect("TR_S", MainMotor.GetWorldPosition());
			GlobalGame.PlayEffect("TR_S", MainMotor.GetWorldPosition() + new Vector2(0, 5));
			SmokeEffectTime = 5;
		}
		TextName.SetWorldPosition(MainMotor.GetWorldPosition() + new Vector2(0, 10));
		string name = Name;
		for (int i = 0; i < WeaponList.Count; i++) {
			name += "[" + WeaponList[i].Ammo + "]";
		}
		TextName.SetText(name);
		if (!HackingProtection && IsJamming(Team)) {
			TextName.SetTextColor(Color.White);
			if (GlobalRandom.Next(100) <= 10) {
				string line = " ";
				for (int i = 0; i < 10; i++) {
					switch (GlobalRandom.Next(5)) {
					case 0: line += "#"; break;
					case 1: line += "@"; break;
					case 2: line += "%"; break;
					case 3: line += "_"; break;
					case 4: line += "*"; break;
					}
				}
				TextName.SetText(line);
			}
		} else if (Team == PlayerTeam.Independent || (!HackingProtection && IsHacking(Team))) TextName.SetTextColor(Color.White);
		else if (Team == PlayerTeam.Team1) TextName.SetTextColor(new Color(122, 122, 224));
		else if (Team == PlayerTeam.Team2) TextName.SetTextColor(new Color(224, 122, 122));
		else if (Team == PlayerTeam.Team3) TextName.SetTextColor(new Color(112, 224, 122));
		if (UpdateHealth()) return;
		UpdateWeapon();		
		if (!HaveAmmo() || (IsJamming(Team) && !HackingProtection)) return;		
		if (LastTargetFinding == 0) {
			PlayerTeam team = Team;
			if (IsHacking(Team) && !HackingProtection) team = PlayerTeam.Independent;
			List <IObject> targetList;
			if (IsProtector) targetList = GetFriendList(team, MainMotor.GetWorldPosition(), GetMaxDistance());
			else targetList = GetTargetList(team, MainMotor.GetWorldPosition(), GetMaxDistance(), CanHitTurret(), HackingProtection);
			Target = null;
			TargetVision = 3;
			for (int i = 0; i < targetList.Count; i++) {
				IObject obj = targetList[i];
				if (obj == MainMotor) continue;
				if (RotationLimit > 0) {
					float angle = TwoPointAngle(MainMotor.GetWorldPosition(), obj.GetWorldPosition());						
					if (Math.Abs(GetAngleDistance(angle, DefaultAngle + MainMotor.GetAngle())) > RotationLimit) continue;
				}
				int trace = TraceToObject(obj);
				if (trace < TargetVision) {
					TargetVision = trace;
					Target = obj;	
					break;		
				}
			}
			LastTargetFinding = 10;
		} else {
			LastTargetFinding--;
		}		
		if (EnableMovement) { 
			if (Target != null &&(MainMotor.GetWorldPosition() - Target.GetWorldPosition()).Length() <= DroneMinDistance) {
				CurrentPath.Clear();
				StopMovement();
			} else	if (LastPathFinding == 0) {
				CurrentPath.Clear();
				StopMovement();
				FindPathToTarget();
				LastPathFinding = 200;
			} 
			if (LastPathFinding > 0) {
				LastPathFinding--;
			}
			Movement();
		}
		if (Target == null) {
			MainMotor.SetMotorSpeed(0);	
			return;	
		}	
		Vector2 targetPos = Target.GetWorldPosition();
		if (IsPlayer(Target.Name)) targetPos = GetPlayerCenter((IPlayer)Target);
		float targetAngle = TwoPointAngle(MainMotor.GetWorldPosition(), targetPos);	
		targetAngle = GetAngleDistance(targetAngle, MainBlockAngle + MainBlock.GetAngle());			
		if (Math.Abs(targetAngle) > 0.01f * RotationSpeed) {
			if (targetAngle > 0) MainMotor.SetMotorSpeed(RotationSpeed);
			else MainMotor.SetMotorSpeed(-RotationSpeed);
		} else {
			MainMotor.SetMotorSpeed(0);
			MainBlock.SetAngle(MainBlock.GetAngle() + targetAngle);
			Fire(TargetVision);
		}
	}
	public void FindPathToTarget() {
		PlayerTeam team = Team;
		if (IsHacking(Team)) team = PlayerTeam.Independent;
		List <IObject> targetList = GetTargetList(team, MainMotor.GetWorldPosition(), 2000, CanHitTurret(), HackingProtection);
		IObject nearestTarget = null;
		float distance = 2000;
		for (int i = 0; i < targetList.Count; ++i) {
			float dist = (targetList[i].GetWorldPosition() - MainMotor.GetWorldPosition()).Length();
			if (dist < distance) {
				nearestTarget = targetList[i];
				distance = dist;
			}
		}
		if (nearestTarget == null) return;
		Vector2 targetCell = GetNearestDroneMapCell(nearestTarget.GetWorldPosition(), PathSize);
		Vector2 currentCell = MainMotor.GetWorldPosition() - DroneAreaBegin;
		currentCell.X = (int)currentCell.X / 8;
		currentCell.Y = (int)currentCell.Y / 8;
		CurrentPath = FindDronePath(currentCell, targetCell, PathSize);
	}
	public void Movement() {
		if (!EnableMovement || CurrentPath.Count == 0) {
			StopMovement();
			return;
		}
		Vector2 position = MainMotor.GetWorldPosition();
		if ((position - CurrentPath[CurrentPath.Count - 1]).Length() <= 1 || RailJoint == null) {			
			CurrentPath.RemoveAt(CurrentPath.Count - 1);		
			if (CurrentPath.Count == 0) {
				StopMovement();
				return;
			} else StopMovement(false);			
			RailJoint = (IObjectRailJoint)GlobalGame.CreateObject("RailJoint", position);
			TargetObject = (IObjectTargetObjectJoint)GlobalGame.CreateObject("TargetObjectJoint", CurrentPath[CurrentPath.Count - 1]);
			RailAttachment = (IObjectRailAttachmentJoint)GlobalGame.CreateObject("RailAttachmentJoint", position);
			RailJoint.SetTargetObjectJoint(TargetObject);			
			RailAttachment.SetRailJoint(RailJoint);
			RailAttachment.SetTargetObject(Hull);
			RailAttachment.SetMotorEnabled(true);
			RailAttachment.SetMaxMotorTorque(10);
			RailAttachment.SetMotorSpeed(Speed);
			Hull.SetBodyType(BodyType.Dynamic);
		}
	}
	public void StopMovement(bool makeStatic = true) {
		if (RailJoint == null) return;		
		RailAttachment.SetTargetObject(null);
		RailAttachment.SetRailJoint(null);
		if (makeStatic) Hull.SetBodyType(BodyType.Static);
		RailJoint.Remove();
		TargetObject.Remove();
		RailAttachment.Remove();
		RailJoint = null;
	}
	public bool UpdateHealth() {
		for (int i = 0; i < DamagedObjects.Count; i++) {			
			if (DamagedObjects[i] == null) {
				Destroy();
				return true;
			}
			float ch = (DamagedObjectMaxHp[i] - DamagedObjects[i].GetHealth()) * DamageFactor;
			DamagedObjectHp[i] -= ch;
			DamagedObjects[i].SetHealth(DamagedObjectMaxHp[i]);			
			if (DamagedObjectHp[i] <= 0) {
				Destroy();
				return true;
			}
		}
		return false;
	}
	public void Destroy() {	
		StopMovement();	
		GlobalGame.TriggerExplosion(MainMotor.GetWorldPosition());
		for (int i = 0; i < DamagedObjects.Count; i++) {
			DamagedObjects[i].Destroy();
		}		
		for (int i = 0; i < OtherObjects.Count; i++) {
			OtherObjects[i].Destroy();
		}
		DamagedObjects.Clear();
		OtherObjects.Clear();
	}
	public void Remove() {
		for (int i = 0; i < DamagedObjects.Count; i++) {
			DamagedObjects[i].Remove();
		}		
		for (int i = 0; i < OtherObjects.Count; i++) {
			OtherObjects[i].Remove();
		}
	}
	public void UpdateWeapon() {
		for (int i = 0; i < WeaponList.Count; i++) {
			if (WeaponList[i].CurrentReloading > 0) WeaponList[i].CurrentReloading--;
			if (WeaponList[i].FireDelay > 0) { 
				WeaponList[i].FireDelay--;
				if (WeaponList[i].FireDelay == 0) WeaponList[i].BulletCount = WeaponList[i].MaxBulletCount;
			}
		}
	}
	public void Fire(int type) {		
		float dist = 2000;
		if (Target != null) dist = (Target.GetWorldPosition() - MainMotor.GetWorldPosition()).Length();
		for (int i = 0; i < WeaponList.Count; i++) {
			if (WeaponList[i].CurrentReloading == 0 && WeaponList[i].Ammo > 0 && dist <= WeaponList[i].Distance && (type < 2  || (type == 2 && WeaponList[i].SuppressiveFire && WeaponList[i].BulletCount > 0))) {
				WeaponList[i].Ammo--;
				if (type == 2) {
					WeaponList[i].BulletCount--;
					if (WeaponList[i].BulletCount == 0) {
						WeaponList[i].FireDelay = WeaponList[i].MaxFireDelay;
					}
				}
				WeaponList[i].CurrentReloading = WeaponList[i].ReloadingTime;
				CreateProjectile(WeaponList[i]);							
			}
		}

	}
	public void CreateProjectile(TTurretWeapon weapon) {
		float angle = MainBlock.GetAngle() + MainBlockAngle + GlobalRandom.Next(-weapon.Scatter, weapon.Scatter + 1) / 180.0f * (float)Math.PI;
		Vector2 pos = MainMotor.GetWorldPosition();
		Vector2 dir = new Vector2((float)Math.Cos(angle) * 10, (float)Math.Sin(angle) * 10);
		if (weapon.BulletType >= 0) {
			GlobalGame.SpawnProjectile((ProjectileItem)weapon.BulletType, pos + dir, dir);
		} else if (weapon.BulletType == -1) {
			GlobalGame.SpawnFireNode(pos + dir, dir * 2, FireNodeType.Flamethrower);
		} else if (weapon.BulletType == -2) {
			ElectricExplosion(MainMotor.GetWorldPosition(), 75, 50);
		}
		GlobalGame.PlaySound(weapon.Sound, MainMotor.GetWorldPosition(), 1.0f);
	}
	public int TraceToObject(IObject obj) {
		float angle = TwoPointAngle(MainMotor.GetWorldPosition(), obj.GetWorldPosition());
		Vector2 tracePoint = MainMotor.GetWorldPosition();
		tracePoint.X += (float)Math.Cos(angle) * 20;
		tracePoint.Y += (float)Math.Sin(angle) * 20;
		PlayerTeam team = Team;
		if (IsHacking(Team)) team = team = PlayerTeam.Independent;
		Vector2 targetPos = obj.GetWorldPosition();
		if (IsPlayer(obj.Name)) targetPos = GetPlayerCenter((IPlayer)obj);
		return TracePath(tracePoint, targetPos, team);
	}
}

public static void CreateTurret(int id, Vector2 position, int dir, PlayerTeam team) {
	TTurret turret = new TTurret(id, position, dir, team);
	TurretList.Add(turret);
}

public class TEquipment {
	public int Id = 0;
	public int AccessLevel = 0;
	public int Cost = 0;
	public int Level = 0;
	public string Name = "";
	public string Description = "";
	public TEquipment(int id, int cost, int level, string name, string description, int accessLevel = 0) {
		Id = id;
		Cost = cost;
		Level = level;
		Name = name;
		Description = description;
		AccessLevel = accessLevel;
	}
}

public class TLevel {
	public string Name;
	public int NeedExp;
	public int AllowPoints;
	public TLevel(string name, int needExp, int allowPoints) {
		Name = name;
		NeedExp = needExp;
		AllowPoints = allowPoints;
	}
}

public class TCustomArmor {
	public int Id = 0;
	public float ProjectileDamageFactor = 1;
	public float FallDamageFactor = 1;
	public float MeleeDamageFactor = 1;
	public float ExplosionDamageFactor = 1;
	public float FireDamageFactor = 1;	
	public bool FireProtect = false;
	public bool SucideMine = false;
	public bool Jammer = false;
	public bool Heavy = false;
	public float MaxProjectileDamage = 75f;
	public float MaxProjectileDamageCut = 27 * 5.75f;
	public float BreakWeaponFactor = 1;
	public float MaxMeleeDamage = 50f;	
	public void SetId(int id) {
		Id = id;
		ProjectileDamageFactor = 1;
		FallDamageFactor = 1;
		MeleeDamageFactor = 1;
		ExplosionDamageFactor = 1;
		FireDamageFactor = 1;
		BreakWeaponFactor = 1;	
		FireProtect = false;
		SucideMine = false;
		Jammer = false;
		Heavy = false;
		MaxProjectileDamage = 75f;
		MaxProjectileDamageCut = 27 * 5.75f;
		MaxMeleeDamage = 50;
		switch (id) {
			case 1: {
				ProjectileDamageFactor = 0.65f;
				MeleeDamageFactor = 0.65f;
				ExplosionDamageFactor = 0.65f;
				break;
			} case 2: {
				FireDamageFactor = 0.25f;
				FireProtect = true;
				break;
			} case 3: {
				SucideMine = true;
				break;
			} case 4: {
				Jammer = true;
				break;
			} case 5: {
				ExplosionDamageFactor = 0.075f;
				break;
			} case 6: {
				Heavy = true;
				ProjectileDamageFactor = 0.3f;
				MeleeDamageFactor = 0.3f;
				MaxMeleeDamage = 25;
				ExplosionDamageFactor = 0.1f;
				BreakWeaponFactor = 0.3f;
				break;
			} case 7: {
				MaxProjectileDamageCut = 5000f;
				MaxProjectileDamage = 50f;
				break;
			}
		}
	}
}


public class TCustomEquipment {
	public int Id = 0;
	public int Reloading = 0;
	public int FastReloading = 0;
	public bool IsActive = false;
	public int Stage = 0;
	public string Name = "";
	public bool ReloadUse = false;
	public int MaxAmmo = 1;
	public int CurrentAmmo;
	public int AmmoRegeneration = 0;
	public Vector2 TargetPosition;
	public Vector2 BeginPosition;
	public List <IObject> ObjectList = new List <IObject>();
	public List <float> OtherData = new List <float>();
	public bool ForceShowReloading = false;
	public TCustomEquipment(int id = 0) {
		SetId(id);
	}
	public void SetId(int id) {
		Id = id;
		ReloadUse = false;
		Reloading = 0;
		FastReloading = 0;
		IsActive = false;
		Stage = 0;
		MaxAmmo = 1;
		AmmoRegeneration = 0;
		if (Id == 0) {
			Name = "";
			return;
		}
		if (Id == 4 || Id == 5 || Id == 6 || Id == 8 || Id == 9 || id == 10) Reloading = 5;
		if (Id == 2) 	MaxAmmo = 3;
		if (Id == 17) {
			OtherData.Add(25);
			OtherData.Add(25);
			OtherData.Add(25);
			OtherData.Add(25);
			OtherData.Add(0);
		}
		if (Id == 20) {
			MaxAmmo = 10;
			OtherData.Add(0);
		}
		CurrentAmmo = MaxAmmo;
		Name = EquipmentList[6].Get(id).Name;
	}
	public string GetName() {
		string name = Name;
		if (MaxAmmo > 1) name += "[" + CurrentAmmo.ToString() + "]";
		if (ForceShowReloading || (Reloading > 0 && !ReloadUse && !IsActive)) name += "(" + Reloading.ToString() + ")";
		return name;
	}
	public void Reload(TPlayer player) {
		if (CurrentAmmo < MaxAmmo && AmmoRegeneration > 0) {
			CurrentAmmo = Math.Min(CurrentAmmo + AmmoRegeneration, MaxAmmo);
		}
		if (Reloading > 0) {
			Reloading--;
		} else if (ReloadUse) {
			Use(player);
			ReloadUse = false;
		}
	}	
	public void DestroyObjects() {
		for (int i = 0; i < ObjectList.Count; i++) {
			ObjectList[i].Destroy();
		}
		ObjectList.Clear();
	}
	public void MinusAmmo() {
		CurrentAmmo--;
		if (CurrentAmmo <= 0 && AmmoRegeneration <= 0) {
			SetId(0);
			return;
		}
	}
	public void Update(TPlayer player) {
		if (FastReloading > 0) {
			FastReloading--;
		}
		switch (Id) {
			case 4: {
				if (IsActive) {
					ContinueNapalmStrike();
				}	
				break;
			}
			case 6: {
				if (IsActive) {
					ContinueAirstrike();
				}
				break;
			} case 8: {
				if (IsActive) {
					ContinueArtilleryStrike();
				}
				break;
			} case 9: {
				if (IsActive) {
					ContinueMineStrike();
				}
				break;
			} case 17: {
				UpdatePoliceShield(player);				
				break;
			} case 20: {
				UpdateJetPack(player);
				break;
			}
		}
	}
	public void Use(TPlayer player) {
		if (Reloading > 0 || FastReloading > 0 || CurrentAmmo <= 0) return;
		switch (Id) {
			case 1: {
				if (RevivePlayer(player)) {}
				else if (StopBleedingSelf(player)) {}
				else if (StopBleedingNear(player)) {}
				else return;
				MinusAmmo();
				break;
			} case 2: {		
				if (RevivePlayer(player)) {}
				else if (StopBleedingSelf(player)) {}
				else if (StopBleedingNear(player)) {}
				else return;
				Reloading = 5;
				MinusAmmo();
				break;
			} case 3: {
				if (!IsJamming(player.Team)) {	
					CallAirDrop(player);
					MinusAmmo();
				}			
				break;
			} case 4: {				
				if (!ReloadUse && !IsActive) {
					if (!IsJamming(player.Team)) {
						ReloadUse = true;
						Reloading = 3;
						GlobalGame.RunCommand("MSG NAPALM STRIKE IS COMING");
					}
				} else {
					if (!IsHacking(player.Team)) IsActive = true;
					else {
						MinusAmmo();
						GlobalGame.RunCommand("MSG NAPALM STRIKE HAS BEED HACKED");
					}
					
				}
				break;
			} case 5: {
				if (!ReloadUse) {
					if (CheckAirPlayer(player, 1) && !IsJamming(player.Team)) {
						ReloadUse = true;
						Reloading = 3;
						if (player.Team == PlayerTeam.Team1) {
								GlobalGame.RunCommand("MSG BLUE TEAM CALLED PINPOINT STRIKE");
							} else {
								GlobalGame.RunCommand("MSG RED TEAM CALLED PINPOINT STRIKE");
							}
					}
				} else {
					if (!CallPinpointStrike(player)) { 
						GlobalGame.RunCommand("MSG PINPOINT STRIKE: TARGET LOST");
						ReloadUse = false;	
						Reloading = 5;					
					} else {
						MinusAmmo();
					}
				}
				break;
			} case 6: {
				if (!IsActive) {
					if (!ReloadUse) {
						if (CheckAirPlayer(player, 2) && !IsJamming(player.Team)) {
							ReloadUse = true;
							Reloading = 3;
							if (player.Team == PlayerTeam.Team1) {
								GlobalGame.RunCommand("MSG BLUE TEAM CALLED AIRSTRIKE");
							} else {
								GlobalGame.RunCommand("MSG RED TEAM CALLED AIRSTRIKE");
							}
						}
						} else {
						if (CallAirstrike(player)) IsActive = true;
						else { 
							GlobalGame.RunCommand("MSG AIRSTRIKE: TARGET LOST");
							ReloadUse = false;
							Reloading = 5;
						}
					}
				}
				break;

			} case 7: {
				if (!IsJamming(player.Team)) {
					CallAirDrop(player, 3);
					MinusAmmo();
				}
				break;
			} case 8: {
				if (!ReloadUse && !IsActive) {
					if (!IsJamming(player.Team)) {
						ReloadUse = true;
						Reloading = 3;
						GlobalGame.RunCommand("MSG ARTILLERY STRIKE IS COMING");
					}
				} else {
					if (!IsHacking(player.Team)) IsActive = true;
					else {
						MinusAmmo();
						GlobalGame.RunCommand("MSG ARTILLERY STRIKE HAS BEED HACKED");						
					}
				}
				break;
			} case 9: {
				if (!ReloadUse && !IsActive) {
					if (!IsJamming(player.Team)) {
						ReloadUse = true;
						Reloading = 3;
						GlobalGame.RunCommand("MSG MINE STRIKE IS COMING");
					}
				} else {
					if (!IsHacking(player.Team)) IsActive = true;
					else {
						MinusAmmo();
						GlobalGame.RunCommand("MSG MINE STRIKE HAS BEED HACKED");						
					}
				}
				break;
			}  case 10: {
				if (!IsJamming(player.Team)) {
					CallReinforcement(player);
					SetId(0);
					if (player.Team == PlayerTeam.Team1) {
						GlobalGame.RunCommand("MSG BLUE TEAM CALLED REINFORCEMENT");
					} else {
						GlobalGame.RunCommand("MSG RED TEAM CALLED REINFORCEMENT");
					}
				}
				break;
			} case 11: {
				TeamJamming[(int)player.Team - 1] += 10;
				MinusAmmo();
				if (player.Team == PlayerTeam.Team1) {
					GlobalGame.RunCommand("MSG BLUE TEAM ENABLE SUPPLY JAMMER");
				} else {
					GlobalGame.RunCommand("MSG RED TEAM ENABLE SUPPLY JAMMER");
				}
				break;
			} case 12: {
				TeamHacking[(int)player.Team - 1] += 10;
				MinusAmmo();
				if (player.Team == PlayerTeam.Team1) {
					GlobalGame.RunCommand("MSG BLUE TEAM ENABLE SUPPLY HACKING");
				} else {
					GlobalGame.RunCommand("MSG RED TEAM ENABLE SUPPLY HACKING");
				}
				break;
			} case 13: {				
				MinusAmmo();
				PlaceTurret(player, 0);
				break;
			} case 14: {
				MinusAmmo();
				PlaceTurret(player, 1);
				break;
			} case 15: {
				MinusAmmo();
				PlaceTurret(player, 2);
				break;
			} case 16: {
				MinusAmmo();
				PlaceTurret(player, 3);
				break;
			} case 18: {
				if (!ReloadUse) {
					if (player.IsAdrenaline) {
						MinusAmmo();
					} else {
						ReloadUse = true;
						Reloading = 5;
						ForceShowReloading = true;
						player.IsAdrenaline = true;
						player.AdrenalineDamageFactor = 0.2f;
						player.DamageDelaySpeed = 1; 
						GlobalGame.PlaySound("GetHealthSmall", player.Position, 1);
					}
				} else {
					player.IsAdrenaline = false;
					MinusAmmo();
				}
				break;
			} case 19: {
				MinusAmmo();
				PlaceShieldGenerator(player);
				break;
			}
		}		
	}
	public bool StopBleedingNear(TPlayer player) {
		for (int i = 0; i < PlayerList.Count; i++) {
			TPlayer pl = PlayerList[i];
			if (pl.User.GetPlayer() != null && pl.Status == 0 && pl.Bleeding == true
			&& pl.Team == player.Team && TestDistance(player.User.GetPlayer().GetWorldPosition(), pl.User.GetPlayer().GetWorldPosition(), 10)) {
				GlobalGame.PlaySound("GetHealthSmall", player.User.GetPlayer().GetWorldPosition(), 1);
				pl.Bleeding = false;
				player.AddExp(2.5f, 1);
				return true;
			}
		}
		return false;	
	}
	public bool StopBleedingSelf(TPlayer player) {
		if (player.Bleeding) {
			GlobalGame.PlaySound("GetHealthSmall", player.User.GetPlayer().GetWorldPosition(), 1);
			player.Bleeding = false;
			return true;
		} else {
			return false;
		}
	}
	public bool RevivePlayer(TPlayer player) {
		for (int i = 0; i < PlayerList.Count; i++) {
			TPlayer pl = PlayerList[i];
			if (!pl.CanRevive()) continue;
			if (pl.Team == player.Team && TestDistance(player.User.GetPlayer().GetWorldPosition(), pl.User.GetPlayer().GetWorldPosition(), 10)) {
				GlobalGame.PlaySound("GetHealthSmall", player.User.GetPlayer().GetWorldPosition(), 1);
				pl.Revive(pl.ReviveHealth);
				player.AddExp(5, 1);
				return true;
			}
		}
		return false;		
	}
	public void CallAirDrop(TPlayer player, int count = 1) {
		Vector2 pos = player.User.GetPlayer().GetWorldPosition();
		if (IsHacking(player.Team)) {
			TPlayer enemy = GetRandomPlayer(GetEnemyTeam(player.Team), true);
			if (enemy != null) pos = enemy.User.GetPlayer().GetWorldPosition();
			else pos = GetRandomWorldPoint();
		}
		pos.Y = WorldTop;
		int offset = 20;
		pos.X -= offset * (count - 1);
		for (int i = 0; i < count; i++) {
			GlobalGame.CreateObject("SupplyCrate00", pos);
			pos.X += offset;
			pos.Y += GlobalRandom.Next(-offset, offset);
		}
	}
	public Vector2 GetRandomAirEnemy(TPlayer player, PlayerTeam friendTeam, int id, ref int angle) {
		angle = 0;
		List <int> indexList = new List<int>();
		for (int i = 0; i < AirPlayerList.Count; i++) {
			if (AirPlayerList[i].Player != null && (friendTeam != AirPlayerList[i].Player.GetTeam() || friendTeam == PlayerTeam.Independent) && !AirPlayerList[i].Player.IsDead) {
				for (int j = 0; j < AirPlayerList[i].StrikeList.Count; j++) {
					if (AirPlayerList[i].StrikeList[j].Id == id) {
						angle = AirPlayerList[i].StrikeList[j].Angle;
						indexList.Add(i);
					}
				}
			}
		}
		if (indexList.Count == 0) return new Vector2(0, 0);
		int rnd = GlobalRandom.Next(indexList.Count);
		return AirPlayerList[indexList[rnd]].Player.GetWorldPosition();
	}
	public Vector2 GetBeginPointTarget(Vector2 target, int angle) {
		Vector2 position = target;
		position.Y = WorldTop;
		position.X += (int)(Math.Tan(angle / 180.0f * Math.PI) * Math.Abs(WorldTop - target.Y));
		return position;
	}
	public void ContinueNapalmStrike() {
		Stage++;
		int continuing = 100;
		int bulletPer = 10;
		if (Stage % bulletPer == 0) {
			Area area = GlobalGame.GetCameraArea();
			Vector2 newPos = new Vector2(GlobalRandom.Next((int)area.Left, (int)area.Right), WorldTop); 
			GlobalGame.CreateObject("WpnMolotovsThrown", newPos);
		}	
		if (Stage >= continuing) {
			MinusAmmo();
		}
	}
	public bool CallPinpointStrike(TPlayer player) {
		int angle = 0;
		PlayerTeam team = player.Team;
		if (IsHacking(player.Team)) {
			team = GetEnemyTeam(player.Team);
			GlobalGame.RunCommand("MSG PINPOINT STRIKE HAS BEEN HACKED");
		}
		Vector2 target = GetRandomAirEnemy(player, team, 1, ref angle);
		if (target.X == 0 && target.Y == 0) {
			if (IsHacking(player.Team)) target = GetRandomWorldPoint();
			else return false;
		}
		if (IsJamming(player.Team)) { 
			target.X += GlobalRandom.Next(-99, 100);
			GlobalGame.RunCommand("MSG PINPOINT STRIKE HAS BEEN JAMMED");
		}
		else target.X += GlobalRandom.Next(-12, 13);
		Vector2 position = GetBeginPointTarget(target, angle);		
		GlobalGame.SpawnProjectile(ProjectileItem.BAZOOKA, position, (target - position));
		return true;
	}
	public bool CallAirstrike(TPlayer player) {
		int angle = 0;
		PlayerTeam team = player.Team;
		if (IsHacking(player.Team)) { 
			team = GetEnemyTeam(player.Team);
			GlobalGame.RunCommand("MSG AIRSTRIKE HAS BEEN HACKED");
		}
		Vector2 target = GetRandomAirEnemy(player, team, 2, ref angle);
		if (target.X == 0 && target.Y == 0) {
			if (IsHacking(player.Team)) target = GetRandomWorldPoint();
			else return false;
		}
		if (IsJamming(player.Team)) { 
			target.X += GlobalRandom.Next(-99, 100);
			GlobalGame.RunCommand("MSG PINPOINT STRIKE HAS BEEN JAMMED");
		}
		Vector2 position = GetBeginPointTarget(target, angle);
		TargetPosition = target;
		BeginPosition = position;
		return true;
	}
	public void ContinueAirstrike() {
		Stage++;
		int continuing = 100;
		int bulletPer = 5;
		int rocketPer = 50;
		if (Stage % bulletPer == 0) {
			Vector2 newPos = TargetPosition;
			newPos.X += GlobalRandom.Next(-16, 17);
			GlobalGame.SpawnProjectile(ProjectileItem.SNIPER, BeginPosition, (newPos - BeginPosition));
		}	
		if (Stage % rocketPer == 0) {
			Vector2 newPos = TargetPosition;
			newPos.X += GlobalRandom.Next(-16, 17);
			GlobalGame.SpawnProjectile(ProjectileItem.BAZOOKA, BeginPosition, (newPos - BeginPosition));

		}
		if (Stage >= continuing) {
			MinusAmmo();
		}
	}
	public void ContinueArtilleryStrike() {
		Stage++;
		int continuing = 100;
		int bulletPer = 10;
		if (Stage % bulletPer == 0) {
			Area area = GlobalGame.GetCameraArea();
			Vector2 newPos = new Vector2(GlobalRandom.Next((int)area.Left, (int)area.Right), WorldTop); 
			GlobalGame.SpawnProjectile(ProjectileItem.GRENADE_LAUNCHER, newPos, new Vector2(0, -1));
		}	
		if (Stage >= continuing) {
			MinusAmmo();
		}
	}
	public void ContinueMineStrike() {
		Stage++;
		int continuing = 70;
		int bulletPer = 10;
		if (Stage % bulletPer == 0) {
			Area area = GlobalGame.GetCameraArea();
			Vector2 newPos = new Vector2(GlobalRandom.Next((int)area.Left, (int)area.Right), WorldTop); 
			GlobalGame.CreateObject("WpnMineThrown", newPos);
		}	
		if (Stage >= continuing) {
			MinusAmmo();
		}
	}
	public bool CheckAirPlayer(TPlayer player, int id) {
		List <int> indexList = new List<int>();
		for (int i = 0; i < AirPlayerList.Count; i++) {
			if (AirPlayerList[i].Player != null && (player.Team != AirPlayerList[i].Player.GetTeam() || player.Team == PlayerTeam.Independent) && !AirPlayerList[i].Player.IsDead) {
				for (int j = 0; j < AirPlayerList[i].StrikeList.Count; j++) {
					if (AirPlayerList[i].StrikeList[j].Id == id) {
						return true;
					}
				}
			}
		}
		return false;
	}
	public void CallReinforcement(TPlayer player) {
		Area area = GlobalGame.GetCameraArea();
		for (int i = 0; i < PlayerList.Count; i++) {
			TPlayer pl = PlayerList[i];
			if (pl != null && pl.IsActive() && pl != player && pl.Team == player.Team && !pl.IsAlive()) {
				float x = GlobalRandom.Next((int)(area.Left + area.Width / 5), (int)(area.Right - area.Width / 5));
				float y = WorldTop + 200;
				IObject crate = GlobalGame.CreateObject("SupplyCrate00", new Vector2(x, y));
				IObject platf = GlobalGame.CreateObject("Lift00A", new Vector2(x, y - 10));
				IObject leftBorder = GlobalGame.CreateObject("Duct00C_D", new Vector2(x - 10, y), (float)Math.PI / 2);
				IObject rightBorder = GlobalGame.CreateObject("Duct00C_D", new Vector2(x + 10, y), (float)Math.PI / 2);
				IObjectDestroyTargets destroy = (IObjectDestroyTargets)GlobalGame.CreateObject("DestroyTargets", new Vector2(x, y));
				platf.SetMass(1e-3f);
				leftBorder.SetMass(1e-3f);
				rightBorder.SetMass(1e-3f);
				IObjectWeldJoint joint = (IObjectWeldJoint)GlobalGame.CreateObject("WeldJoint", new Vector2(x, y));
				joint.AddTargetObject(crate);
				joint.AddTargetObject(platf);
				joint.AddTargetObject(rightBorder);
				joint.AddTargetObject(leftBorder);
				destroy.AddTriggerDestroyObject(crate);
				destroy.AddObjectToDestroy(joint);
				destroy.AddObjectToDestroy(platf);
				destroy.AddObjectToDestroy(leftBorder);
				destroy.AddObjectToDestroy(rightBorder);
				ObjectToRemove.Add(destroy);
				ObjectToRemove.Add(platf);
				ObjectToRemove.Add(joint);
				ObjectToRemove.Add(leftBorder);
				ObjectToRemove.Add(rightBorder);
				pl.Equipment.Clear();		
				pl.Armor.SetId(0);
				pl.Revive(100, true, x, y);
				player.AddExp(5, 5);
			}
		}
	}
	public void PlaceTurret(TPlayer player, int id) {
		Vector2 position = player.User.GetPlayer().GetWorldPosition();
		position += new Vector2(player.User.GetPlayer().FacingDirection * 10, 15);
		CreateTurret(id, position, player.User.GetPlayer().FacingDirection, player.Team);
	}
	public void PlaceShieldGenerator(TPlayer player) {
		Vector2 position = player.User.GetPlayer().GetWorldPosition();
		position += new Vector2(player.User.GetPlayer().FacingDirection * 10, 15);
		CreateShieldGenerator(50, position, 50, player.Team);
	}
	public void SpawnDrone(TPlayer player, int id) {
		Area area = GlobalGame.GetCameraArea();
		float x = GlobalRandom.Next((int)(area.Left + area.Width / 5), (int)(area.Right - area.Width / 5));
		float y = area.Top + 10;
		CreateTurret(id, new Vector2(x, y), player.User.GetPlayer().FacingDirection, player.Team);		
	}
	public void UpdateJetPack(TPlayer player) {
		if (CurrentAmmo <= 0) {
			SetId(0);
			return;
		}
		IPlayer pl = player.User.GetPlayer();
		if (pl == null) return;
		Vector2 pos = pl.GetWorldPosition();
		bool velChange = false;
		Vector2 vel = pl.GetLinearVelocity();		
		if (pl.IsWalking && !pl.IsDiving && !pl.IsClimbing && !pl.IsManualAiming  && Reloading == 0 && CurrentAmmo >= 2) {
			Reloading = 5;
			velChange = true;
			vel.Y = 13;	
			CurrentAmmo -= 2;
			OtherData[0] = 20;		
		} else if (!pl.IsDiving && !pl.IsFalling && vel.Y < -13) {
			velChange = true;
			vel.Y = -6.5f;
			OtherData[0] = 5;
			CurrentAmmo -= 1;
		}
		if (velChange) {		
			if (vel.X != 0) vel.X = Math.Min(3, Math.Abs(vel.X)) * Math.Abs(vel.X) / vel.X;	
			pl.SetLinearVelocity(vel);	
		}
		if (OtherData[0] > 0) {			
			DrawJetPackEffect(player);
			OtherData[0]--;
		}
	}
	public void DrawJetPackEffect(TPlayer player) {
		IPlayer pl = player.User.GetPlayer();
		Vector2 vel = pl.GetLinearVelocity();
		Vector2 pos = pl.GetWorldPosition();
		if (vel.Y < 0) {
			pos.Y -= 6;
			GlobalGame.PlayEffect("FIRE", pos + new Vector2(10, 0));
			GlobalGame.PlayEffect("FIRE", pos - new Vector2(10, 0));
		} else {
			pos.Y -= 8;
			GlobalGame.PlayEffect("FIRE", pos);
		}
		GlobalGame.PlaySound("Flamethrower", pos, 1f);
	}
	public void UpdatePoliceShield(TPlayer player) {
		IPlayer pl = player.User.GetPlayer();
		if (pl == null) return;
		Vector2 pos = pl.GetWorldPosition();
		int dir = pl.FacingDirection;
		Vector2 shieldPos = pos + new Vector2(10 * dir, 4);
		Area area = new Area(shieldPos.Y + 4, shieldPos.X - 2, shieldPos.Y, shieldPos.X);		
		if (ObjectList.Count > 0) {				
			float damageFactor = 0.125f;
			if (!CheckAreaToCollision(area)) {
				ObjectList[0].SetWorldPosition(pos + new Vector2(10 * dir, 4));
				ObjectList[1].SetWorldPosition(pos + new Vector2(8 * dir, 17));
				ObjectList[1].SetAngle(0.3f * dir);
			}
			float ch = (OtherData[0] - ObjectList[0].GetHealth()) * damageFactor;
			OtherData[1] -= ch;
			ObjectList[0].SetHealth(OtherData[0]);
			ch = (OtherData[2] - ObjectList[1].GetHealth()) * damageFactor;
			OtherData[3] -= ch;
			ObjectList[1].SetHealth(OtherData[2]);
			if (ObjectList[0].DestructionInitiated || OtherData[1] <= 0 || ObjectList[1].DestructionInitiated || OtherData[3] <= 0) {
				ObjectList[0].Destroy();
				ObjectList[1].Destroy();
				MinusAmmo();
				ObjectList.Clear();
				return;
			}
		}
		if (!pl.IsDead && pl.FacingDirection == OtherData[4] && !pl.IsManualAiming && !pl.IsHipFiring && !pl.IsClimbing && !pl.IsDiving && !pl.IsRolling && !pl.IsStaggering && !pl.IsTakingCover && !pl.IsInMidAir && !pl.IsLayingOnGround && !pl.IsLedgeGrabbing && !pl.IsMeleeAttacking && pl.IsWalking) {
			if (ObjectList.Count == 0 && !CheckAreaToCollision(area)) {				
				IObject shield = GlobalGame.CreateObject("MetalRailing00", pos + new Vector2(10 * dir, 4));
				ObjectList.Add(shield);
				shield = GlobalGame.CreateObject("MetalRailing00", pos + new Vector2(8 * dir, 17), 0.3f * dir);
				ObjectList.Add(shield);
			}			
		} else {
			if (pl.FacingDirection != OtherData[4]) {
				OtherData[4] = pl.FacingDirection;
				FastReloading = 10;				
			}
			if (ObjectList.Count > 0) {
				ObjectList[0].Remove();
				ObjectList[1].Remove();
				ObjectList.Clear();
			}
		}
	}
}

public class TPlayer {
	//global values
	public int Level = 0;
	public float CurrentExp = 0;
	//values
	public float ProjectileDamageFactor = 1;
	public float FallDamageFactor = 1;
	public float MeleeDamageFactor = 1;
	public float ExplosionDamageFactor = 1;
	public float FireDamageFactor = 1;	
	public int JumpCount = 0;
	public float MeleeDamageTaken = 0;
	public float ProjectileDamageTaken = 0;
	public float ExplosionDamageTaken = 0;
	public float FallDamageTaken = 0;
	public int BlocksCount = 0;
	public float FireDamageTaken = 0;
	public bool Bleeding = false;
	public int StartBleedingProjectile;
	public int StartBleedingMelee;
	public float Hp = 100;
	public int DyingChance;
	public int AliveChance;
	public int OvercomeChance;
	public int DyingHealth;
	public int StableHealth;
	public int OvercomeHealth;
	public int ReviveHealth;
	public float DyingSpeed;
	public float OvercomeSpeed;
	public int WeaponBreakingChance;
	public int WeaponExplosionChance;
	public int Status = 0; //0 - 682>9 //1 - C<8@05B //2 - AB018;878@>20= //3 - ?@52>7<>305B 
	public bool IsSlow = false;
	public int InSmoke = 0;
	public bool IsAdrenaline = false;
	public float DelayedDamage = 0;
	public float AdrenalineDamageFactor = 0.5f;
	public float DamageDelaySpeed = 0.5f;
	public TCustomArmor Armor = new TCustomArmor();
	public List <TCustomEquipment> Equipment = new List <TCustomEquipment>();
	public int CurrentEquipment = 0;	
	public int StunTime = 0;
	//system
	public IUser User;
	public PlayerTeam Team;
	public IObjectText StatusDisplay;
	public float DiveHeight = 0;
	public bool IsDiving = false;	
	public int DiveTime = 0;
	public float[] ExpSource = {0, 0, 0, 0, 0, 0};
	public bool IsNewLevel = false;
	public int SlowTimer = 0;
	public int BleedingEffectTimer = 0;
	public int SlowEffectTimer = 0;
	public bool ActiveStatus = true;
	public Vector2 Position;
	public string Name;
	//weapon
	public TWeapon PrimaryWeapon = null;
	public TWeapon SecondaryWeapon = null;	
	public TWeapon ThrownWeapon = null;
	//methods
	public TPlayer(IUser user) {	
		User = user;
		Name = User.Name;
		Team = User.GetPlayer().GetTeam();
		StatusDisplay = (IObjectText)GlobalGame.CreateObject("Text");
		StatusDisplay.SetTextAlignment(TextAlignment.Middle);
	}
	public void UpdateActiveStatus() {
		if (User.IsRemoved) {
			ActiveStatus = false;
			return;
		} 
		IUser[] users = GlobalGame.GetActiveUsers();
		for (int i = 0; i < users.Length; i++) {
			if (users[i].Name == User.Name) {
				return;
			}
		}
		ActiveStatus = false;
	}
	
	public bool IsActive() {
		if (!ActiveStatus) return false;
		UpdateActiveStatus();
		return ActiveStatus;
	}
	public bool CanRevive() {
		IPlayer pl = User.GetPlayer();
		return pl != null && Status > 0;
	}
	public bool IsAlive() {
		IPlayer pl = User.GetPlayer();
		return pl != null && (!pl.IsDead || Status == 3 || Status == 4);
	}
	public void AddExp(float value, int type) {
		value *= XPBonus;
		if (Name.Contains("New Player") || Name.Contains("Unnamed")) {
			Level = 0;
			CurrentExp = 0;
			return;
		}
		ExpSource[type] += value;
		CurrentExp += value;
		while (Level + 1 < LevelList.Count && LevelList[Level + 1].NeedExp <= CurrentExp) {
			Level++;
			CurrentExp -= LevelList[Level].NeedExp;
			IsNewLevel = true;
		}
	}	
	public void Respawn(bool full = true) {		
		JumpCount = 0;
		MeleeDamageTaken = 0;
		ProjectileDamageTaken = 0;
		ExplosionDamageTaken = 0;
		FallDamageTaken = 0;
		BlocksCount = 0;
		FireDamageTaken = 0;
		DelayedDamage = 0;
		Bleeding = false;
		Status = 0;
		IsDiving = false;
		StatusDisplay.SetText("");
		PrimaryWeapon = null;
		SecondaryWeapon = null;
		ThrownWeapon = null;
		InSmoke = 0;
		StunTime = 0;
		if (full) {
			Equipment.Clear();
			ProjectileDamageFactor = 5.75f;
			FallDamageFactor = 3.5f;
			MeleeDamageFactor = 2f;
			ExplosionDamageFactor = 7.5f;
			FireDamageFactor = 3;
			Hp = 100;
			DyingChance = 75;
			AliveChance = 50;
			OvercomeChance = 25;
			WeaponBreakingChance = 15;
			WeaponExplosionChance = 50;
			IsSlow = false;
			StartBleedingProjectile = 15;
			StartBleedingMelee = 20;
			DyingHealth = 5;
			StableHealth = 10;
			OvercomeHealth = 20;
			ReviveHealth = 40;
			DyingSpeed = 0.007f;
			OvercomeSpeed = 0.1f;
			IsAdrenaline = false;
			AdrenalineDamageFactor = 0.5f;
			DamageDelaySpeed = 0.5f;
		}
	}
	public IProfile GetSkin() {
		string skinName = "";
		if (Team == PlayerTeam.Team1) skinName += "Blue";
		else if (Team == PlayerTeam.Team2) skinName += "Red";
		if (Armor.Id == 1) skinName += "Light";
		else if (Armor.Id == 2) skinName += "Fire";
		else if (Armor.Id == 3) skinName += "Sucide";
		else if (Armor.Id == 4) skinName += "Jammer";
		else if (Armor.Id == 5) skinName += "Bomb";
		else if (Armor.Id == 6) skinName += "Heavy";
		else if (Armor.Id == 7) skinName += "Kevlar";
		return ((IObjectPlayerProfileInfo)GlobalGame.GetSingleObjectByCustomId(skinName)).GetProfile();

	}
	public void SetTeam(PlayerTeam team) {
		Team = team;
		IPlayer pl = User.GetPlayer();
		if (pl != null) pl.SetTeam(team);
	}
	public void OnPlayerCreated() {
		IPlayer pl = User.GetPlayer();
		PlayerModifiers mods = pl.GetModifiers();
		mods.ProjectileCritChanceTakenModifier = 0;
		mods.ProjectileCritChanceDealtModifier = 0;
		mods.ProjectileDamageTakenModifier = ProjectileDamageFactor * Armor.ProjectileDamageFactor;
		mods.ExplosionDamageTakenModifier = ExplosionDamageFactor * Armor.ExplosionDamageFactor;
		mods.FireDamageTakenModifier = FireDamageFactor * Armor.FireDamageFactor;
		mods.MeleeDamageTakenModifier = MeleeDamageFactor * Armor.MeleeDamageFactor;
		mods.ImpactDamageTakenModifier = FallDamageFactor * Armor.FallDamageFactor;
		mods.MaxHealth = 10000;
		pl.SetModifiers(mods);
	}
	
	public void Revive(float hp = 100, bool withPos = false, float x = 0, float y = 0) {
		Respawn(false); 
		IPlayer pl = User.GetPlayer();
		Vector2 position;
		if (!withPos) position = pl.GetWorldPosition();
		else position = new Vector2(x, y);
		if (pl != null) pl.Remove(); 
		pl = GlobalGame.CreatePlayer(position);
		pl.SetProfile(GetSkin());
		pl.SetUser(User);
		pl.SetTeam(Team);
		//pl.SetStatusBarsVisible(false);
		Hp = hp;
		OnPlayerCreated();
	}
	public void AddEquipment(int id, int type) {
		IPlayer player = User.GetPlayer();		
		if (player != null && !player.IsDead) {
			if (type == 6) {
				if (id != 0) Equipment.Add(new TCustomEquipment(id));
			} else if (type == 5) {				
				Armor.SetId(id);
			} else if (type == 4) {
				ApplyWeaponMod(this, id);	
			} else if (type == 3) {
				ApplyThrownWeapon(this, id);				
			} else {
				ApplyWeapon(this, (WeaponItem)id);
			}
		}
		WeaponTrackingUpdate(true);
	}
	public void ReloadEquipment() {
		for (int i = 0; i < Equipment.Count; i++) {
			Equipment[i].Reload(this);
		}
	}
	public void Start() {
		IPlayer player = User.GetPlayer();
		if (player != null && !player.IsDead) {
			player.SetInputEnabled(true);
		}		
	}
	public void Stop() {
		IPlayer player = User.GetPlayer();
		if (player != null && !player.IsDead) {
			player.SetInputEnabled(false);
		}
		StatusDisplay.SetText("");
	}
	public string Save() {
		string data = "";
		data += FormatName(Name);
		data += ":" + Level.ToString() + ":" + ((int)CurrentExp).ToString() + ":";
		return data;
	}
	public void BreakWeapon(bool isExplosion) {
		IPlayer pl = User.GetPlayer();
		if (pl != null && !pl.IsDead) {
			if (pl.CurrentWeaponDrawn == WeaponItemType.Rifle) {				
				if (GlobalRandom.Next(0, 100) <  (int)(WeaponBreakingChance * Armor.BreakWeaponFactor)) {
					Vector2 pos =  pl.GetWorldPosition();
					RifleWeaponItem weapon = pl.CurrentPrimaryWeapon;
					if ((weapon.WeaponItem == WeaponItem.BAZOOKA || weapon.WeaponItem == WeaponItem.GRENADE_LAUNCHER
						|| weapon.WeaponItem == WeaponItem.FLAMETHROWER) && weapon.CurrentAmmo > 0 && (GlobalRandom.Next(0, 100) < WeaponExplosionChance || isExplosion)) {
						GlobalGame.TriggerExplosion(pos);						
					}
					pl.RemoveWeaponItemType(WeaponItemType.Rifle);			
					GlobalGame.PlayEffect("CFTXT", pl.GetWorldPosition(), "WEAPON BROKEN");
					GlobalGame.CreateObject("MetalDebris00A", pos,(float)rnd.NextDouble());
					pos.X += 5;
					GlobalGame.CreateObject("MetalDebris00B", pos, (float)rnd.NextDouble());
					pos.X -= 10;
					GlobalGame.CreateObject("MetalDebris00C", pos, (float)rnd.NextDouble());
				}
			}
		}
	}
	public void OnDead() {	
		AddTeamExp(5, 0, GetEnemyTeam(Team), true);
		IPlayer pl = User.GetPlayer();
		if (pl != null) {
			if (Armor.SucideMine) {
				GlobalGame.TriggerExplosion(Position);
			}
		}
	}
	public void Update() {
		if (!IsActive()) return;
		for (int i = 0; i < Equipment.Count; i++) {
			if (Equipment[i].Id == 0) {
				Equipment.RemoveAt(i);
				i--;
			} else {
				Equipment[i].Update(this);
			}
		}
		IPlayer pl = null;
		if (User != null) pl = User.GetPlayer();
		if (pl != null && (pl.RemovalInitiated || pl.IsRemoved)) pl = null;
		if (pl != null && pl.GetProfile().Name == "CPU") {
			pl.Remove();
			ActiveStatus = false;
			return;
		}
		if (pl != null) {
			Position = pl.GetWorldPosition();
			WeaponTrackingUpdate(false);
		}
		if (pl != null && Status >= 0) {	
			if (Status == 0) {
				StatusDisplay.SetWorldPosition(Position + new Vector2(0, 37));
			} else {
				StatusDisplay.SetWorldPosition(Position + new Vector2(0, 15));			
			}
			pl.SetNametagVisible(InSmoke <= 0);
			bool wasBleeding = Bleeding;
			IPlayerStatistics stat = pl.Statistics;
			float lastHp = Hp;
			if (ProjectileDamageTaken < stat.TotalProjectileDamageTaken) {
				float ch = stat.TotalProjectileDamageTaken - ProjectileDamageTaken;
				ProjectileDamageTaken = stat.TotalProjectileDamageTaken;				
				//ch *= ProjectileDamageFactor * Armor.ProjectileDamageFactor;
				if (InSmoke > 0) ch *= 0.25f;
				if (ch < Armor.MaxProjectileDamageCut && ch > Armor.MaxProjectileDamage) ch = Armor.MaxProjectileDamage;
				if (ch >= StartBleedingProjectile) Bleeding = true;
				Hp -= ch;
				if (InSmoke <= 0) BreakWeapon(false);
				
			}			
			if (MeleeDamageTaken < stat.TotalMeleeDamageTaken) {
				float ch = stat.TotalMeleeDamageTaken - MeleeDamageTaken;
				MeleeDamageTaken = stat.TotalMeleeDamageTaken;
				//ch *= MeleeDamageFactor * Armor.MeleeDamageFactor;
				if (ch > Armor.MaxMeleeDamage) ch = Armor.MaxMeleeDamage;
				if (ch >= StartBleedingMelee) Bleeding = true;
				Hp -= ch;
			}
			if (ExplosionDamageTaken < stat.TotalExplosionDamageTaken) {
				float ch = stat.TotalExplosionDamageTaken - ExplosionDamageTaken;
				ExplosionDamageTaken = stat.TotalExplosionDamageTaken;
				//ch *= ExplosionDamageFactor * Armor.ExplosionDamageFactor;
				if (ch >= StartBleedingProjectile) Bleeding = true;
				Hp -= ch;
				BreakWeapon(true);
			}	
			if (FallDamageTaken < stat.TotalFallDamageTaken) {
				float ch = stat.TotalFallDamageTaken - FallDamageTaken;
				FallDamageTaken = stat.TotalFallDamageTaken;
				//ch *= FallDamageFactor * Armor.FallDamageFactor;
				Hp -= ch;
			}
			if (Armor.FireProtect) {
				if (pl.IsBurning) pl.ClearFire();
			} else {
				if (pl.IsBurning) pl.SetMaxFire();
				if (FireDamageTaken < stat.TotalFireDamageTaken) {
					float ch = stat.TotalFireDamageTaken - FireDamageTaken;
					FireDamageTaken = stat.TotalFireDamageTaken;				
					//ch *= FireDamageFactor * Armor.FireDamageFactor;
					Hp -= ch;
				}
			}
			if (Bleeding) {
				if (!wasBleeding && Hp > 0) {
					GlobalGame.PlayEffect("CFTXT", pl.GetWorldPosition(), "BLEEDING");
				}
				if (pl.IsSprinting || pl.IsRolling || pl.IsDiving || pl.IsClimbing || pl.IsMeleeAttacking || pl.IsKicking || pl.IsJumpKicking || pl.IsJumpAttacking) {
					Hp -= HardBleeding;
				} else {
					Hp -= EasyBleeding;
				}
				if (JumpCount < stat.TotalJumps) {
					int ch = stat.TotalJumps - JumpCount;					
					Hp -= ch * JumpBleeding;
				}
				if (BleedingEffectTimer == 0) {
					Vector2 effPos =  pl.GetWorldPosition();
					effPos.Y += 6;
					GlobalGame.PlayEffect("BLD", effPos);
					BleedingEffectTimer = BleedingEffectPeriod;
				}
				if (BleedingEffectTimer > 0) BleedingEffectTimer--;
			}
			if (IsDiving && !pl.IsDiving) {
				IsDiving = false;
				if (DiveTime >= 45) {
					float diff = DiveHeight - pl.GetWorldPosition().Y;
					diff -= MinDivingHeight * DivingDamageFactor * FallDamageFactor;
					if (diff > 0) Hp -= diff;
				}
			} else if (!IsDiving && pl.IsDiving) {
				IsDiving = true;
				DiveTime = 0;
				DiveHeight = pl.GetWorldPosition().Y;
			} else if (IsDiving && pl.IsDiving) {
				DiveTime++;
			}			
			if (IsAdrenaline) {
				float ch = (lastHp - Hp) * AdrenalineDamageFactor;		
				DelayedDamage += (lastHp - Hp) - ch;
				Hp = lastHp - ch;
			} else if (DelayedDamage > 0) {
				if (DamageDelaySpeed > DelayedDamage) {
					Hp -= DelayedDamage;
					DelayedDamage = 0;
				} else {
					Hp -= DamageDelaySpeed;
					DelayedDamage -= DamageDelaySpeed; 
				}
			}
			WeaponItem heavyWeapon = pl.CurrentPrimaryWeapon.WeaponItem;
			if (heavyWeapon == WeaponItem.M60 || heavyWeapon == WeaponItem.GRENADE_LAUNCHER || heavyWeapon == WeaponItem.BAZOOKA || heavyWeapon == WeaponItem.SNIPER || Armor.Heavy) IsSlow = true;
			else IsSlow = false;		
			if (IsSlow) {
				PlayerModifiers mods = pl.GetModifiers();
				 mods.MaxEnergy = 0;
				 mods.MeleeStunImmunity = 1;
				 mods.EnergyRechargeModifier = 0;
				 if (Armor.Heavy) mods.SizeModifier = 1.15f;
				 pl.SetModifiers(mods);
			}
			/*if (IsSlow) {
				if (pl.IsRolling  || pl.IsDiving) {
					Vector2 vel = pl.GetLinearVelocity() / 3;
					vel.Y = 0;
					pl.SetWorldPosition(Position - vel);
					if (SlowEffectTimer == 0) SlowEffectTimer = -1;
				}
				if (SlowTimer == 0 && pl.IsSprinting) {					
					SlowTimer = 4;
					if (SlowEffectTimer == 0) SlowEffectTimer = -1;
				} 
				if (SlowTimer <= 2) {
					pl.SetInputEnabled(true);
				} else {
					pl.SetInputEnabled(false);
				}
				if (SlowEffectTimer == -1) {
					GlobalGame.PlayEffect("CFTXT", pl.GetWorldPosition(), "TOO HEAVY");
					SlowEffectTimer = 100;
				}
				if (SlowEffectTimer > 0) SlowEffectTimer--;
				if (SlowTimer > 0) SlowTimer--;
			} else if (!pl.IsInputEnabled) {
				pl.SetInputEnabled(true);
			}*/
			JumpCount = stat.TotalJumps;
			if (Status == 0) {
				if (StunTime > 0 && Status != 4) {
					Status = 4;
					pl.Kill();
				}
				if (Hp > 0 && StunTime <= 0) {		
					pl.SetHealth(pl.GetMaxHealth() * Hp / 100.0f);
					//if (Hp < lastHp) pl.SetHealth(99 + GlobalRandom.Next(100) / 100.0f);				
					//else pl.SetHealth(100);
					if (pl.IsWalking && !pl.IsManualAiming && !pl.IsClimbing && !pl.IsLedgeGrabbing && !pl.IsInMidAir && !pl.IsRolling) {
						if (CurrentEquipment < Equipment.Count) {
							Equipment[CurrentEquipment].Use(this);
						}
					} else if (Equipment.Count > 1 && pl.CurrentWeaponDrawn == WeaponItemType.NONE) {
						if (pl.IsMeleeAttacking || pl.IsJumpAttacking) CurrentEquipment = 0;
						else if (pl.IsKicking || pl.IsJumpKicking) CurrentEquipment = 1;
					}					
				} else if (Hp <= 0){
					if (!IsActive()) {
						Status = -1;
					} else if (!pl.IsBurning || Armor.FireProtect) {
						int rnd = GlobalRandom.Next(0, 100);
						Bleeding = false;
						if (rnd < OvercomeChance) {						
							Hp = OvercomeHealth;
							Status = 3;
						} else if (rnd < AliveChance) {
							Hp = StableHealth;			
							Status = 2;
						} else if (rnd < DyingChance) {
							Hp = DyingHealth;
							Status = 1;
						} else {
							Status = -1;			
						}
						pl.Kill();
					} else {
						Status = -1;
						pl.Kill();
					}
				} 
			} else if (Status == 1 || Status == 2 || Status == 3) {
				if (pl.IsBurnedCorpse && !Armor.FireProtect) Hp = 0;
				if (Hp <= 0) {
					Status = -1;
				} else if (Hp <= DyingHealth) {
					Status = 1;
				} else if (Hp <= StableHealth) {
					Status = 2;
				} else if (Hp <=  ReviveHealth){
					Status = 3;
				}
			}
			if (Status == 0) {
				StatusDisplay.SetTextColor(Color.White);
			} else {
				if (Team == PlayerTeam.Team1) {
					StatusDisplay.SetTextColor(new Color(128, 128, 255));
				} else {
					StatusDisplay.SetTextColor(new Color(255, 128, 128));
				}
			}
			if (Status == 0) {				
				if (CurrentEquipment < Equipment.Count && InSmoke <= 0) {
					StatusDisplay.SetText(Equipment[CurrentEquipment].GetName());
				} else {
					StatusDisplay.SetText("");
				}
			} else if (Status == 1) {
				StatusDisplay.SetText("DYING");
				Hp -= DyingSpeed;						
			} else if (Status == 2) {
				StatusDisplay.SetText("STABLE");
			} else if (Status == 3) {
				StatusDisplay.SetText("OVERCOMING");
				Hp += OvercomeSpeed;
			} else if (Status == 4) {
				StatusDisplay.SetText("STUN");
				StunTime--;
			}
			if (Status == 3 && Hp >= ReviveHealth) {
				Revive(Hp);
			} else if (Status == 4 && StunTime <= 0) {
				Revive(Hp);
			}
		} else if (pl == null && Status >= 0) {
			Status = -1;
		}
		if (InSmoke > 0) InSmoke--;
		if (Status == -1) {
			Status = -2;
			OnDead();
			StatusDisplay.SetText("");
		}
	}
	public void WeaponTrackingUpdate(bool onlyAdd) {
		IPlayer pl = User.GetPlayer();
		RifleWeaponItem rifle = pl.CurrentPrimaryWeapon;
		HandgunWeaponItem pistol = pl.CurrentSecondaryWeapon;
		ThrownWeaponItem thrown = pl.CurrentThrownItem;
		if (onlyAdd) {
			if (rifle.WeaponItem != WeaponItem.NONE) {
				if (PrimaryWeapon == null) PrimaryWeapon = new TWeapon(rifle.WeaponItem);
				PrimaryWeapon.TotalAmmo = rifle.TotalAmmo;
			}
			if (pistol.WeaponItem != WeaponItem.NONE) {
				if (SecondaryWeapon == null) SecondaryWeapon = new TWeapon(pistol.WeaponItem);
				SecondaryWeapon.TotalAmmo = pistol.TotalAmmo;
			}
			if (thrown.WeaponItem != WeaponItem.NONE) {
				if (ThrownWeapon == null) ThrownWeapon = new TWeapon(thrown.WeaponItem);
				ThrownWeapon.TotalAmmo = thrown.CurrentAmmo;
			}
			return;
		}
		if (rifle.WeaponItem == WeaponItem.NONE && PrimaryWeapon != null) PrimaryWeapon = null;
		else if ((PrimaryWeapon == null && rifle.WeaponItem != WeaponItem.NONE) 
			|| (PrimaryWeapon != null && (rifle.WeaponItem != PrimaryWeapon.Weapon || PrimaryWeapon.TotalAmmo < rifle.TotalAmmo))) {
			TWeapon pickUp = PlayerPickUpWeaponUpdate(Position, rifle.WeaponItem);
			if (pickUp != null)	PrimaryWeapon = pickUp;
			else PrimaryWeapon = new TWeapon(rifle.WeaponItem);
			PrimaryWeapon.TotalAmmo = rifle.TotalAmmo;
		} else if (PrimaryWeapon != null && rifle.TotalAmmo < PrimaryWeapon.TotalAmmo) {
			PrimaryWeapon.TotalAmmo = rifle.TotalAmmo;
			if (!pl.IsReloading) PrimaryWeapon.OnFire(this, WeaponItemType.Rifle);
		}			
		if (pistol.WeaponItem == WeaponItem.NONE && SecondaryWeapon != null) SecondaryWeapon = null;
		else if ((SecondaryWeapon == null && pistol.WeaponItem != WeaponItem.NONE) 
			|| (SecondaryWeapon != null && (pistol.WeaponItem != SecondaryWeapon.Weapon || SecondaryWeapon.TotalAmmo < pistol.TotalAmmo))) {
			TWeapon pickUp = PlayerPickUpWeaponUpdate(Position, pistol.WeaponItem);
			if (pickUp != null)	SecondaryWeapon = pickUp;
			else SecondaryWeapon = new TWeapon(pistol.WeaponItem);
			SecondaryWeapon.TotalAmmo = pistol.TotalAmmo;
		} else if (SecondaryWeapon != null && pistol.TotalAmmo < SecondaryWeapon.TotalAmmo) {
			SecondaryWeapon.TotalAmmo = pistol.TotalAmmo;
			if (!pl.IsReloading) SecondaryWeapon.OnFire(this, WeaponItemType.Handgun);
		}
		if (thrown.WeaponItem == WeaponItem.NONE && ThrownWeapon != null) {			
			if (!IsPlayerDropWeapon(Position, thrown.WeaponItem)) ThrownWeapon.OnFire(this, WeaponItemType.Thrown);
			ThrownWeapon = null;
		}
		else if ((ThrownWeapon == null && thrown.WeaponItem != WeaponItem.NONE) 
			|| (ThrownWeapon != null && (thrown.WeaponItem != ThrownWeapon.Weapon || ThrownWeapon.TotalAmmo < thrown.CurrentAmmo))) {
			TWeapon pickUp = PlayerPickUpWeaponUpdate(Position, thrown.WeaponItem);
			if (pickUp != null) {
				if (ThrownWeapon != null && ThrownWeapon.CustomId != pickUp.CustomId) pickUp.CustomId = 0;
				ThrownWeapon = pickUp;
			} else ThrownWeapon = new TWeapon(thrown.WeaponItem);
			ThrownWeapon.TotalAmmo = thrown.CurrentAmmo;
		} else if (ThrownWeapon != null && thrown.CurrentAmmo < ThrownWeapon.TotalAmmo) {
			ThrownWeapon.TotalAmmo = thrown.CurrentAmmo;
			ThrownWeapon.OnFire(this, WeaponItemType.Thrown);
		}
		if (PrimaryWeapon != null) PrimaryWeapon.Update();
		if (SecondaryWeapon != null) SecondaryWeapon.Update();
		if (ThrownWeapon != null) ThrownWeapon.Update();
	}
}

public TEquipmentSlot AddEquipmentSlot(string name) {
	TEquipmentSlot newSlot = new TEquipmentSlot(name);
	newSlot.AddEquipment(0, 0, 0, "None");
	EquipmentList.Add(newSlot);	
	return newSlot;
}

public void OnStartup() {
	Game.StartupSequenceEnabled = false;
	Game.DeathSequenceEnabled = false;
	GlobalGame = Game;
	GlobalGame.SetAllowedCameraModes(CameraMode.Static);
	BeginTimer = (IObjectText)Game.GetSingleObjectByCustomId("BeginTimer");

	AddUserAccessLevels();
	GlobalGame.RunCommand("IE 1");

	VisionObjects.Add("Concrete01A", 3);
	VisionObjects.Add("Concrete01B", 3);
	VisionObjects.Add("Concrete00A", 3);
	VisionObjects.Add("Concrete00B", 3);
	VisionObjects.Add("Concrete00C", 3);
	VisionObjects.Add("Concrete01C", 3);
	VisionObjects.Add("Concrete02B", 3);
	VisionObjects.Add("Concrete02C", 3);
	VisionObjects.Add("Concrete02K", 3);
	VisionObjects.Add("Stone00C", 3);
	VisionObjects.Add("Concrete02H", 3);
	VisionObjects.Add("Dirt01A", 3);
	VisionObjects.Add("Dirt01C", 3);
	VisionObjects.Add("Dirt01D", 3);
	VisionObjects.Add("Metal06A", 3);
	VisionObjects.Add("Metal06B", 3);
	VisionObjects.Add("Metal06C", 3);
	VisionObjects.Add("Metal02A", 3);
	VisionObjects.Add("Metal02B", 3);
	VisionObjects.Add("Metal02C", 3);
	VisionObjects.Add("HangingCrate00", 3);
	VisionObjects.Add("CargoContainer00A", 3);

	VisionObjects.Add("Crate00", 1);
	VisionObjects.Add("Barrel00", 2);
	VisionObjects.Add("BarrelExplosive", 1);
	VisionObjects.Add("CashRegister00", 1);
	VisionObjects.Add("SwivelChair02", 1);
	VisionObjects.Add("Desk00", 2);
	VisionObjects.Add("FileCab00", 2);
	VisionObjects.Add("Chair00", 1);
	VisionObjects.Add("MetalTable00", 1);
	VisionObjects.Add("Safe00", 2);

	WeaponItemNames.Add(WeaponItem.PISTOL, "WpnPistol");
	WeaponItemNames.Add(WeaponItem.SILENCEDPISTOL, "WpnSilencedPistol");
	WeaponItemNames.Add(WeaponItem.MAGNUM, "WpnMagnum");
	WeaponItemNames.Add(WeaponItem.REVOLVER, "WpnRevolver");
	WeaponItemNames.Add(WeaponItem.SHOTGUN, "WpnPumpShotgun");
	WeaponItemNames.Add(WeaponItem.TOMMYGUN, "WpnTommygun");
	WeaponItemNames.Add(WeaponItem.SMG, "WpnSMG");
	WeaponItemNames.Add(WeaponItem.M60, "WpnM60");
	WeaponItemNames.Add(WeaponItem.SAWED_OFF, "WpnSawedOff");
	WeaponItemNames.Add(WeaponItem.UZI, "WpnUzi");
	WeaponItemNames.Add(WeaponItem.SILENCEDUZI, "WpnSilencedUzi");
	WeaponItemNames.Add(WeaponItem.BAZOOKA, "WpnBazooka");
	WeaponItemNames.Add(WeaponItem.ASSAULT, "WpnAssaultRifle");
	WeaponItemNames.Add(WeaponItem.SNIPER, "WpnSniperRifle");
	WeaponItemNames.Add(WeaponItem.CARBINE, "WpnCarbine");
	WeaponItemNames.Add(WeaponItem.FLAMETHROWER, "WpnFlamethrower");
	WeaponItemNames.Add(WeaponItem.FLAREGUN, "WpnFlareGun");
	WeaponItemNames.Add(WeaponItem.GRENADE_LAUNCHER, "WpnGrenadeLauncher");
	WeaponItemNames.Add(WeaponItem.GRENADES, "WpnGrenades");
	WeaponItemNames.Add(WeaponItem.MOLOTOVS, "WpnMolotovs");
	WeaponItemNames.Add(WeaponItem.MINES, "WpnMines");
	WeaponItemNames.Add(WeaponItem.SHURIKEN, "WpnShuriken");
	WeaponItemNames.Add(WeaponItem.BOW, "WpnBow");
	WeaponItemNames.Add(WeaponItem.SHOCK_BATON, "WpnShockBaton");

	//Game.RunCommand("MSG HARDCORE V2.0");
	//Game.RunCommand("MSG HARDCORE: Loading equipment...");
	Game.RunCommand("MSG GAME DATABASE: SFD-HARDCORE.WIKIA.COM");
	Game.RunCommand("MSG https://discord.gg/Y7zGwNN");

	//slots
	TEquipmentSlot meleeWeaponSlot = AddEquipmentSlot("Melee Weapon");
	TEquipmentSlot secondaryWeaponSlot = AddEquipmentSlot("Secondary Weapon");
	TEquipmentSlot primaryWeaponSlot = AddEquipmentSlot("Primary Weapon");
	TEquipmentSlot thrownWeaponSlot = AddEquipmentSlot("Thrown Weapon");
	TEquipmentSlot weaponModSlot = AddEquipmentSlot("Weapon Mod");
	TEquipmentSlot bodySlot = AddEquipmentSlot("Body");
	TEquipmentSlot equipmentSlot = AddEquipmentSlot("Equipment");

	//weapon
	meleeWeaponSlot.AddEquipment(8, 0, 0, "Machete"); //1
	meleeWeaponSlot.AddEquipment(49, 25, 3, "Knife", "Can be thrown.");
	meleeWeaponSlot.AddEquipment(4, 0, 0, "Pipe"); //2
	meleeWeaponSlot.AddEquipment(11, 0, 0, "Baseball Bat"); //3
	meleeWeaponSlot.AddEquipment(31, 0, 0, "Hammer"); //4
	meleeWeaponSlot.AddEquipment(18, 25, 3, "Axe", "Big damage. Good way to end the fight. Can be thrown."); //5
	meleeWeaponSlot.AddEquipment(41, 0, 0, "Baton"); //6
	meleeWeaponSlot.AddEquipment(3, 50, 5, "Katana", "Huge damage. Can be thrown."); //7
	meleeWeaponSlot.AddEquipment(57, 50, 7, "Shock Baton"); //8
	
	secondaryWeaponSlot.AddEquipment(24, 50, 0, "Pistol"); //1
	secondaryWeaponSlot.AddEquipment(39, 50, 0, "Silenced Pistol"); //2
	secondaryWeaponSlot.AddEquipment(12, 75, 1, "Uzi"); //3
	secondaryWeaponSlot.AddEquipment(40, 75, 1, "Silenced Uzi"); //4
	secondaryWeaponSlot.AddEquipment(27, 50, 6, "Flare Gun"); //5
	secondaryWeaponSlot.AddEquipment(28, 150, 9, "Revolver"); //6
	secondaryWeaponSlot.AddEquipment(1, 175, 14, "Magnum"); //7

	primaryWeaponSlot.AddEquipment(5, 100, 2, "Tommy Gun"); //1
	primaryWeaponSlot.AddEquipment(10, 100, 0, "Sawed-Off Shotgun"); //2
	primaryWeaponSlot.AddEquipment(30, 100, 0, "SMG"); //3
	primaryWeaponSlot.AddEquipment(23, 100, 2, "Carbine", "Very accurate weapon with good damage, but low fire rate.");	//4
	primaryWeaponSlot.AddEquipment(19, 125, 5, "Assault Rifle", "Good damage and accuracy. Medium fire rate."); //5
	primaryWeaponSlot.AddEquipment(2, 125, 4, "Shotgun");	//6
	primaryWeaponSlot.AddEquipment(26, 125, 8, "Flamethrower"); //7
	primaryWeaponSlot.AddEquipment(29, 150, 13, "Grenade Launcher"); //8
	primaryWeaponSlot.AddEquipment(6, 150, 15, "M60", "Big damage and fire rate, but low accuracy. Too heavy, you cant sprint."); //9
	primaryWeaponSlot.AddEquipment(9, 150, 13, "Sniper Rifle", "Best accurate weapon with huge damage. Too heavy, you cant sprint."); //10
	primaryWeaponSlot.AddEquipment(17, 150, 12, "Bazooka"); //11
	//primaryWeaponSlot.AddEquipment(64, 150, 9, "Bow"); //12

	thrownWeaponSlot.AddEquipment(1, 50, 6, "Grenades"); //1	
	thrownWeaponSlot.AddEquipment(2, 25, 5, "Molotovs"); //2
	thrownWeaponSlot.AddEquipment(3, 50, 7, "Mines"); //3
	thrownWeaponSlot.AddEquipment(4, 100, 10, "Incendiary grenades"); //4
	thrownWeaponSlot.AddEquipment(5, 25, 5, "Smoke grenades", "", 1); //5
	thrownWeaponSlot.AddEquipment(6, 50, 7, "Flashbang", "", 1); //6
	thrownWeaponSlot.AddEquipment(7, 50, 3, "Shuriken"); //7
	
	weaponModSlot.AddEquipment(1, 25, 3, "Lazer Scope", "Helps to aim precisely."); //1
	weaponModSlot.AddEquipment(2, 25, 4, "Extra Ammo", "Add extra ammo to your light weapon."); //2
	weaponModSlot.AddEquipment(3, 50, 8, "Extra Explosives", "Add extra ammo to your explosive weapon."); //3
	weaponModSlot.AddEquipment(4, 50, 10, "Extra Heavy Ammo", "Add extra ammo to your heavy weapon."); //4
	weaponModSlot.AddEquipment(5, 25, 4, "DNA Scanner", "If the enemy try to shoot from your gun, it will explode.", 1); //4
	//weaponModSlot.AddEquipment(6, 100, 11, "Bouncing ammo"); //5
	
	//equipment
	equipmentSlot.AddEquipment(1, 25, 1, "Small Medkit", "Allows one time stop the bleeding or revive teammate."); //1
	equipmentSlot.AddEquipment(2, 50, 3, "Big Medkit", "Allows 3 times stop the bleeding or revive teammate."); //2
	equipmentSlot.AddEquipment(3, 25, 1, "Airdrop", "Drops one supply crate with random weapon."); //3
	equipmentSlot.AddEquipment(4 , 100, 10, "Napalm Strike", "Strike of Napalm bombs on the whole map."); //4
	equipmentSlot.AddEquipment(5, 100, 11, "Pinpoint Strike", "The missile tries to hit your enemy."); //5
	equipmentSlot.AddEquipment(6, 100, 14, "Airstrike", "Attack aircraft tries to hit your enemy."); //6
	equipmentSlot.AddEquipment(7, 50, 7, "Big Airdrop", "Drops three supply crates with random weapon."); //7
	equipmentSlot.AddEquipment(8, 125, 13, "Artillery Strike", "150 mm cannons bombards the all map."); //8
	equipmentSlot.AddEquipment(9, 50, 8, "Mine Strike", "Mines are falling from the air all over the map."); //9
	equipmentSlot.AddEquipment(10, 250, 15, "Reinforcement", "Revives all your dead teammates and drops them by parachute."); //10
	equipmentSlot.AddEquipment(11, 25, 9, "Supply Jammer", "Your enemies cant call supply while jammer is working. Jammer working 10 seconds."); //11
	equipmentSlot.AddEquipment(12, 75, 11, "Supply Hacking", "Try to hack enemy supply. Who knows what will be after ?"); //12
	equipmentSlot.AddEquipment(13, 150, 11, "Light Turret", "Automatically shoots at enemies in range of the minigun");
	equipmentSlot.AddEquipment(14, 175, 14, "Rocket Turret", "Automatically shoots at enemies in range of the rocket launcher");
	equipmentSlot.AddEquipment(15, 200, 15, "Heavy Turret", "Automatically shoots at enemies in range. It have minigun and rocket launcher.");
	equipmentSlot.AddEquipment(16, 175, 14, "Sniper Turret", "Automatically shoots at enemies in range of the sniper.", 1);
	equipmentSlot.AddEquipment(17, 50, 4, "Police Shield", "Protects you from some bullets.");
	equipmentSlot.AddEquipment(18, 50, 3, "Adrenaline", "Gives temporary immunity to damage.You will receive all damage when adrenaline is over.", 1);
	//equipmentSlot.AddEquipment(19, 200, 15, "Shield Generator", "Creates an energy shield that protects from bullets and enemies.", 2);
	equipmentSlot.AddEquipment(20, 100, 15, "Jet Pack", "Allows you to make jet jumps. And protect from falling.");

	//armor
	bodySlot.AddEquipment(1, 50, 2, "Light Armor", "Decrease the damage a bit."); //1
	bodySlot.AddEquipment(2, 50, 7, "Fire Suit", "Protects you from fire."); //2
	bodySlot.AddEquipment(3, 25, 6, "Suicide Vest", "Leaves a small surprise after your death. "); //3
	bodySlot.AddEquipment(4, 50, 12, "Personal Jammer", "You cant be a target for strikes."); //4
	bodySlot.AddEquipment(5, 50, 10, "Blast Suit", "Decrease the explosion damage."); //5
	bodySlot.AddEquipment(6, 150, 12, "Heavy Armor", "Decrease the damage greatly. Very heavy. You cant sprint and roll."); //6
	bodySlot.AddEquipment(7, 50, 9, "Kevlar Armor", "Protects you from oneshot death."); //7

	{
		//human 0
		AddLevel("Private", 0, 100);
		AddLevel("First Private", 100, 125);
		AddLevel("Specialist", 120, 150);
		AddLevel("Corporal", 140, 175);
		AddLevel("Sergeant", 160, 200);
		AddLevel("Staff Sergeant", 180, 200);
		AddLevel("Master Sergeant", 200, 225);
		AddLevel("Master Sergeant II", 220, 225);
		AddLevel("First Sergeant", 240, 250);
		AddLevel("Second Lieutenant", 260, 275);
		AddLevel("First Lieutenant", 280, 275);
		AddLevel("Captain", 300, 300);
		AddLevel("Major", 320, 325);
		AddLevel("Colonel", 340, 350);
		AddLevel("Colonel II", 360, 350);
		AddLevel("General", 380, 350);
	}

	//Game.RunCommand("MSG HARDCORE: Loading maps...");
	
	for (int i = 0; i < 3; i++) {
		MapPartList.Add(new TMapPart());
	}
	//1
	MapPartList[0].MapPosition = new Vector2(-1120, 256);
	MapPartList[0].PointList.Add(new TCapturePoint((IObjectText)Game.GetSingleObjectByCustomId("Point00"), -100));
	MapPartList[0].PointList.Add(new TCapturePoint((IObjectText)Game.GetSingleObjectByCustomId("Point01"), 100));
	MapPartList[0].PointList.Add(new TCapturePoint((IObjectText)Game.GetSingleObjectByCustomId("Point02"), 0));
	//MapPartList[0].PointList.Add(new TCapturePoint((IObjectText)Game.GetSingleObjectByCustomId("Point03"), 0));
	for (int i = 0; i < 8; i++) {
		MapPartList[0].RedSpawnPosition.Add(Game.GetSingleObjectByCustomId("RedSpawnPoint0" + i));
		MapPartList[0].BlueSpawnPosition.Add(Game.GetSingleObjectByCustomId("BlueSpawnPoint0" + i));
	}


	//2
	MapPartList[1].MapPosition = new Vector2(-352, 256);
	MapPartList[1].PointList.Add(new TCapturePoint((IObjectText)Game.GetSingleObjectByCustomId("Point10"), -100));
	MapPartList[1].PointList.Add(new TCapturePoint((IObjectText)Game.GetSingleObjectByCustomId("Point11"), 100));
	MapPartList[1].PointList.Add(new TCapturePoint((IObjectText)Game.GetSingleObjectByCustomId("Point12"), 0));
	for (int i = 0; i < 8; i++) {
		MapPartList[1].RedSpawnPosition.Add(Game.GetSingleObjectByCustomId("RedSpawnPoint1" + i));
		MapPartList[1].BlueSpawnPosition.Add(Game.GetSingleObjectByCustomId("BlueSpawnPoint1" + i));
	}

	//3
	MapPartList[2].MapPosition = new Vector2(416, 256);
	MapPartList[2].PointList.Add(new TCapturePoint((IObjectText)Game.GetSingleObjectByCustomId("Point20"), -100));
	MapPartList[2].PointList.Add(new TCapturePoint((IObjectText)Game.GetSingleObjectByCustomId("Point21"), 100));
	MapPartList[2].PointList.Add(new TCapturePoint((IObjectText)Game.GetSingleObjectByCustomId("Point22"), 0));
	//MapPartList[2].PointList.Add(new TCapturePoint((IObjectText)Game.GetSingleObjectByCustomId("Point23"), 0));
	for (int i = 0; i < 8; i++) {
		MapPartList[2].RedSpawnPosition.Add(Game.GetSingleObjectByCustomId("RedSpawnPoint2" + i));
		MapPartList[2].BlueSpawnPosition.Add(Game.GetSingleObjectByCustomId("BlueSpawnPoint2" + i));
	}
	GenerateDroneMap();
	//Game.RunCommand("MSG HARDCORE: Loading players...");
	IUser[] users = Game.GetActiveUsers();
	int menuCounter = 0;
	for (int i = 0; i < users.Length; i++) {
		if (users[i].IsSpectator) continue;
		TPlayer player = new TPlayer(users[i]);
		PlayerList.Add(player);	
		TPlayerMenu menu = new TPlayerMenu();		
		menu.Menu = (IObjectText)Game.GetSingleObjectByCustomId("PlayerMenu" + menuCounter.ToString());
		PlayerMenuList.Add(menu);	
		menuCounter++;
	}
	//if (Game.Data == "") Game.Data = SavedData;
	LoadData();
	for (int i = 0; i < PlayerList.Count; i++) {
		PlayerMenuList[i].SetPlayer(PlayerList[i]);
	}
	for (int i = PlayerMenuList.Count; i < 8; i++) {
		Game.GetSingleObjectByCustomId("PlayerMenu" + i.ToString()).Remove();
	}	
	TeamBalance();
	CurrentMapPartIndex = 1;
	CameraPosition.X = Game.GetCameraArea().Left;
	CameraPosition.Y = Game.GetCameraArea().Top;
	UpdateTrigger =  (IObjectTrigger)Game.CreateObject("OnUpdateTrigger", new Vector2(0, 0), 0);
	UpdateTrigger.SetScriptMethod("OnUpdate");
	BeginTimerTrigger =  (IObjectTimerTrigger)Game.CreateObject("TimerTrigger", new Vector2(0, 0), 0);
	BeginTimerTrigger.SetRepeatCount(0);
	BeginTimerTrigger.SetIntervalTime(1000);
	BeginTimerTrigger.SetScriptMethod("OnBeginTimer");
	BeginTimerTrigger.Trigger();
}

public void OnUpdate(TriggerArgs args) {


	if (GameState == 0) {
		for (int i = 0; i < PlayerMenuList.Count; i++) {
			PlayerMenuList[i].Update();
		}
	} else if (GameState == 1) {	
		GameState = 2;	
		AirPlayerList.Clear();	
		ThrownTrackingList.Clear();			
		RemoveObjects();
		RemoveTurrets();
		RemoveShieldGenerators();
		ResetElectronicWarfare();	
		ResetEffects();
		RemoveWeapons(); 
		MapPartList[CurrentMapPartIndex].Start();	
		TimeToStart = AreaTime;	
	} else if (GameState == 2) {
		if (Game.GetCameraArea().Left == CameraPosition.X && Game.GetCameraArea().Top == CameraPosition.Y) {
			for (int i = 0; i < PlayerList.Count; i++) {
				PlayerList[i].Start();
			}			
			Game.RunCommand("MSG BATTLE BEGINS");
			GameState = 3;
		}
	} else if (GameState == 3) {
		if (!IsDebug && PlayerList.Count < 2) {
			Game.SetGameOver("NOT ENOUGH PLAYERS");
			GameState = 100;
			return;
		}
		int areaStatus = MapPartList[CurrentMapPartIndex].Update();
		int capturedBy = MapPartList[CurrentMapPartIndex].CapturedBy;
		UpdateEffects();
		ThrownWeaponUpdate();
		PreWeaponTrackingUpdate();
		for (int i = 0; i < PlayerList.Count; i++) {
			PlayerList[i].Update();
		}
		PostWeaponTrackingUpdate();
		UpdateTurrets();
		UpdateShieldGenerators();
		ThrowingUpdate();
		if (IsAllPlayerDead()) {
			GameState = 6;
			TimeToStart = 5;
		} else if (TimeToStart <= 0) {
			if (capturedBy == 0) SpawnDrone(4, PlayerTeam.Team3);
			else if (capturedBy == 1) SpawnDrone(4, PlayerTeam.Team1);
			else if (capturedBy == 2) SpawnDrone(4, PlayerTeam.Team2);
			TimeToStart = 30;
		}
		int teamStatus = IsOneTeamDead();
		if (teamStatus != 0 || GameState == 6) {
			for (int i = 0; i < PlayerList.Count; i++) {
				PlayerList[i].Stop();
			}
			for (int i = 0; i < TurretList.Count; i++) {
				TurretList[i].StopMovement();
			}
		}
		if (areaStatus == 1) {
			Game.RunCommand("MSG BLUE TEAM CAPTURE ALL POINTS");			
			SpawnDrone(4, PlayerTeam.Team1);
			TimeToStart = 30;
		} else if (areaStatus == 2) {
			Game.RunCommand("MSG RED TEAM CAPTURE ALL POINTS");
			SpawnDrone(4, PlayerTeam.Team2);
			TimeToStart = 30;
		}
		if (teamStatus == 1) {
			TimeToStart = 5;
			GameState = 7;
		} else if (teamStatus == 2) {
			TimeToStart = 5;
			GameState = 8;
		}
		
	} else if (GameState == 4) {
		Game.RunCommand("MSG BLUE CAPTURE THE AREA");
		AddTeamExp(10, 3, PlayerTeam.Team1, false);
		if (CurrentMapPartIndex > 0) {
			CurrentMapPartIndex--;
			GameState = 1;
		} else {			
			AddTeamExp(20, 4, PlayerTeam.Team1, false);
			GameState = -1;
			TimeToStart = 15;
		}			
		SaveData();
	} else if (GameState == 5) {
		Game.RunCommand("MSG RED CAPTURE THE AREA");
		AddTeamExp(10, 3, PlayerTeam.Team2, false);
		if (CurrentMapPartIndex < MapPartList.Count - 1) {
			CurrentMapPartIndex++;
			GameState = 1;
		} else {
			AddTeamExp(20, 4, PlayerTeam.Team2, false);
			GameState = -2;
			TimeToStart = 15;			
		}			
		SaveData();
	} else if (GameState == 6) {
		if (TimeToStart <= 0) {
			Game.RunCommand("MSG NOBODY CAPTURE THE AREA");
			GameState = 1;
		}
	} else if (GameState == 7) {
		if (TimeToStart <= 0) {
			GameState = 4;
		}
	} else if (GameState == 8) {
		if (TimeToStart <= 0) {
			GameState = 5;
		}
	} else if (GameState == -1 || GameState == -2) {
		CameraPosition.X = -352;
		CameraPosition.Y = 768;
		for (int i = 0; i < PlayerMenuList.Count; i++) {
			PlayerMenuList[i].ShowExp();
		}		
		GlobalGame.SetWeatherType(WeatherType.None);
		GameState -= 2;
	} else if (GameState == -3) {
		if (TimeToStart <= 0) {
			Game.SetGameOver("BLUE TEAM WINS");
		}
	} else if (GameState == -4) {
		if (TimeToStart <= 0) {
			Game.SetGameOver("RED TEAM WINS");
		}
	}
	UpdateCamera();
}

public void OnBeginTimer(TriggerArgs args) {
	if (TimeToStart > 0) {
		TimeToStart--;
	}
	if (GameState == 0) {
		int readyPlayers = 0;
		for (int i = 0; i < PlayerMenuList.Count; i++) {
			if (PlayerMenuList[i].Ready) {
				readyPlayers++;
			}
		}
		if (TimeToStart > 10 && (float)readyPlayers / (float)PlayerMenuList.Count > 2.0 / 3.0) {
			if (IsDebug) TimeToStart = 0;
			else TimeToStart = 10;
		}		
		if (TimeToStart <= 10) {			
			BeginTimer.SetTextColor(Color.Red);
			GlobalGame.PlaySound("TimerTick", new Vector2(0, 600), 1.0f);
		}
		BeginTimer.SetText("Choose your equipment: " + TimeToStart.ToString());
		if (TimeToStart == 0) {
			GameState = 1;
			CameraPosition.Y -= 512;
			BeginTimer.SetText("");
		}
	} else if (GameState == 3) {
		for (int i = 0; i < PlayerList.Count; i++) {
			PlayerList[i].ReloadEquipment();
		}
		for (int i = 0; i < TeamJamming.Length; i++) {
			if (TeamJamming[i] > 0) {
				TeamJamming[i]--;
				if (TeamJamming[i] == 0) {
					if (i == 0) Game.RunCommand("MSG RED TEAM DISABLE SUPPLY JAMMER");
					else Game.RunCommand("MSG BLUE TEAM DISABLE SUPPLY JAMMER");
				}
			}
		}
		for (int i = 0; i < TeamHacking.Length; i++) {
			if (TeamHacking[i] > 0) {
				TeamHacking[i]--;
				if (TeamHacking[i] == 0) {
					if (i == 0) Game.RunCommand("MSG RED TEAM DISABLE SUPPLY HACKING");
					else Game.RunCommand("MSG BLUE TEAM DISABLE SUPPLY HACKING");
				}
			}
		}
	}
	if (GameState != 0) {
		if (!IsDebug) {
			Game.ShowPopupMessage("Time: " + TimeToStart.ToString());
		}
	}
}

public void UpdateCamera() {
	Area cameraArea = Game.GetCameraArea();	
	if (cameraArea.Left != CameraPosition.X) {
		if (Math.Abs(cameraArea.Left - CameraPosition.X) <= CameraSpeed) {
			cameraArea.Left = CameraPosition.X;
		} else if (cameraArea.Left < CameraPosition.X) {
			cameraArea.Left += CameraSpeed;
		} else {
			cameraArea.Left -= CameraSpeed;
		}
		cameraArea.Right = cameraArea.Left + 768;
	}
	if (cameraArea.Top != CameraPosition.Y) {
		if (Math.Abs(cameraArea.Top - CameraPosition.Y) <= CameraSpeed) {
			cameraArea.Top = CameraPosition.Y;
		} else if (cameraArea.Top < CameraPosition.Y) {
			cameraArea.Top += CameraSpeed;
		} else {
			cameraArea.Top -= CameraSpeed;
		}
		cameraArea.Bottom = cameraArea.Top - 512;
	}
	Game.SetCameraArea(cameraArea);
	//Game.SetBorderArea(cameraArea);
}

public static bool TestDistance(Vector2 p1, Vector2 p2, int radius) {
	return (Math.Abs(p1.X - p2.X) <= radius) && (Math.Abs(p1.Y - p2.Y) <= radius);
}

public static bool IsAllPlayerDead() {
	if (IsDebug) return false;
	for (int i = 0; i < PlayerList.Count; i++) {
		if (PlayerList[i].IsAlive() && PlayerList[i].Team != PlayerTeam.Independent) {
			return false;
		}
	}
	return true;
}

public static int IsOneTeamDead() {
	if (IsDebug) return 0;
	bool team1 = false;
	bool team2 = false;
	for (int i = 0; i < PlayerList.Count; i++) {
		if (PlayerList[i].IsAlive()) {
			if (PlayerList[i].Team == PlayerTeam.Team1) {
				team1 = true;
			} else if (PlayerList[i].Team == PlayerTeam.Team2) {
				team2 = true;
			}
		}
	}
	if (!team2 && team1) return 1;
	else if (!team1 && team2) return 2;
	else return 0;
}
public static void SaveData() {
	string data = OtherData;
	for (int i = 0; i < PlayerMenuList.Count; i++) {
		data += PlayerMenuList[i].Save();
	}
	GlobalGame.LocalStorage.SetItem("SaveData", data);
	//GlobalGame.Data = "BEGIN" + "DATA" + data + "ENDDATA";
}

public static void LoadData() {
	string data = "";
	bool status = GlobalGame.LocalStorage.TryGetItemString("SaveData", out data);
	if (!status) data = "";
	data = data.Replace("BEGIN" + "DATA", "").Replace("ENDDATA", "");
	string[] playerList = data.Split(';');
	for (int i = 0; i < playerList.Length; i++) {
		string[] plData = playerList[i].Split(':');
		if (plData.Length == 11) {
			List <string> list = new List <string>(plData);
			list.RemoveAt(3);
			plData = list.ToArray();
		}
		for (int j = 0; j < PlayerList.Count; j++) {
			string name = FormatName(PlayerList[j].Name);
			if (name == plData[0]) {		
				PlayerMenuList[j].SetPlayer(PlayerList[j]);
				PlayerList[j].Level = Math.Min(Convert.ToInt32(plData[1]), LevelList.Count - 1);
				PlayerList[j].CurrentExp = Convert.ToInt32(plData[2]);
				for (int k = 0; k < PlayerMenuList[j].Equipment.Count; k++) {
					PlayerMenuList[j].Equipment[k] = Convert.ToInt32(plData[3 + k]);
				}
				PlayerMenuList[j]. ValidateEquipment();
				playerList[i] = "";
				break;
			}
		}
	}
	for (int i = 0; i < playerList.Length; i++) {
		if (playerList[i] != "") {
			OtherData += playerList[i] + ";";
		}
	}
}

public static void AddTeamExp(int exp, int type, PlayerTeam team, bool aliveOnly) {
	for (int i = 0; i < PlayerList.Count; i++) {
		if (PlayerList[i].Team == team && (PlayerList[i].IsAlive() || !aliveOnly)) {	
			PlayerList[i].AddExp(exp, type);
		}
	}
}

public int StrikeNameToInt(string name) {
	if (name == "Napalm") return 0;
	if (name == "Pinpoint") return 1;
	if (name == "Airstrike") return 2;
	return 0;
}

public bool CanStrikeJamming(int id) {
	if (id >= 0 && id <= 2) return true;
	else return false;
}

public void AirAreaEnter(TriggerArgs args) {
	string data = ((IObject)args.Caller).CustomId;
	IPlayer pl = (IPlayer)args.Sender;
	TPlayer player = GetPlayer(pl);	
	if (player == null) return;
	string[] typeList = data.Split(';');
	TPlayerStrikeInfo plInfo = new TPlayerStrikeInfo();
	plInfo.Player = pl;
	for (int i = 0; i < typeList.Length; i++) {
		string[] strikeInfo = typeList[i].Split(':');
		int id = StrikeNameToInt(strikeInfo[0]);
		if (CanStrikeJamming(id) && player.Armor.Jammer) continue;
		int angle = 0;
		if (strikeInfo.Length > 1) angle = Convert.ToInt32(strikeInfo[1]);
		TStrikeInfo info = new TStrikeInfo();
		info.Id = id;
		info.Angle = angle;
		plInfo.StrikeList.Add(info);
	}
	if (plInfo.StrikeList.Count > 0) AirPlayerList.Add(plInfo);
}

public void AirAreaLeave(TriggerArgs args) {
	IPlayer pl = (IPlayer)args.Sender;
	for (int i = 0; i < AirPlayerList.Count; i++) {
		if (AirPlayerList[i].Player == pl) {
			AirPlayerList.RemoveAt(i);
			return;
		}
	}
}

public static TPlayer GetPlayer(IPlayer player) {
	for (int i = 0; i < PlayerList.Count; i++) {
		IPlayer pl = PlayerList[i].User.GetPlayer();
		if (pl != null && pl == player) return PlayerList[i];	
	}
	return null;
}

public void RemoveObjects() {
	string []name = {"WpnMineThrown", "SupplyCrate00", "MetalRailing00", "WpnGrenadesThrown"};
	IObject []objects = GlobalGame.GetObjectsByName(name);
	for (int i = 0; i < objects.Length; i++) {
		objects[i].Remove();
	}
	for (int i = 0; i < ObjectToRemove.Count; i++) {
		if (ObjectToRemove[i] != null) {
			ObjectToRemove[i].Remove();
		}
	}
	ObjectToRemove.Clear();
}

public static PlayerTeam GetEnemyTeam(PlayerTeam team) {
	if (team == PlayerTeam.Team1) return PlayerTeam.Team2;
	else return PlayerTeam.Team1;
}

public static void ResetElectronicWarfare() {
	TeamJamming = new int[] {0, 0, 0};
	TeamHacking = new int[] {0, 0, 0};
}

public static TPlayer GetRandomPlayer(PlayerTeam team, bool isAlive) {
	List <TPlayer> players = new List <TPlayer>();
	for (int i = 0; i < PlayerList.Count; i++) {
		TPlayer player = PlayerList[i];
		if (player.Team == team && player.IsAlive() == isAlive) players.Add(player);
	}
	if (players.Count == 0) return null;
	int index = GlobalRandom.Next(0, players.Count);
	return players[index];
}

public static Vector2 GetRandomWorldPoint() {
	Area area = GlobalGame.GetCameraArea();
	return new Vector2(GlobalRandom.Next((int)area.Left, (int)area.Right),
			 GlobalRandom.Next((int)area.Bottom, (int)area.Top));
}

public static void UpdateEffects() {
	for (int i = 0; i < EffectList.Count; i++) {
		if (!EffectList[i].Update()) {
			EffectList.RemoveAt(i);
			i--;
		}
	}
}

public static void ResetEffects() {
	EffectList.Clear();
}

public static void CreateEffect(IObject obj, string id, int time, int count) {
	TEffect effect = new TEffect(obj, id, time, count);
	EffectList.Add(effect);
}

public static bool IsPlayer(string name) {
	for (int i = 0; i < PlayerList.Count; i++) {
		IPlayer pl = PlayerList[i].User.GetPlayer();
		if (pl == null) continue;
		if (pl.Name == name) return true;
	}
	return false;
}

public static void TeamBalance() {
	List <KeyValuePair <float, int> > levelList = new List <KeyValuePair <float, int> >();
	for (int i = 0; i < PlayerList.Count; i++) {
		levelList.Add(new KeyValuePair <float, int>(PlayerList[i].Level / 4.0f + 1, i));
	}
	levelList.Sort((x, y) => x.Key.CompareTo(y.Key));
	levelList.Reverse();
	float teamPower1 = 0;
	float teamPower2 = 0;
	for (int i = 0; i < levelList.Count; i++) {		
		if (teamPower1 < teamPower2 || (teamPower1 == teamPower2 && GlobalRandom.Next(0,2) == 0)) {
		teamPower1 += levelList[i].Key;			
			PlayerList[levelList[i].Value].SetTeam(PlayerTeam.Team1);
		} else {
			teamPower2 += levelList[i].Key;
			PlayerList[levelList[i].Value].SetTeam(PlayerTeam.Team2);
		}
	}
}

public static void UpdateShieldGenerators() {
	for (int i = 0; i < ShieldGeneratorList.Count; i++) {
		ShieldGeneratorList[i].Update();
		if (ShieldGeneratorList[i].CoreObject == null) {
			ShieldGeneratorList.RemoveAt(i);
			i--;
		}
	}
}

public static void RemoveShieldGenerators() {
	for (int i = 0; i < ShieldGeneratorList.Count; i++) {
		ShieldGeneratorList[i].Remove();
	}
	ShieldGeneratorList.Clear();
}

public static void UpdateTurrets() {
	for (int i = 0; i < TurretList.Count; i++) {
		TurretList[i].Update();
		if (TurretList[i].OtherObjects.Count == 0) {
			TurretList.RemoveAt(i);
			i--;
		}
	}
}

public static void RemoveTurrets() {
	for (int i = 0; i < TurretList.Count; i++) {
		TurretList[i].Remove();
	}
	TurretList.Clear();
}


public static int TracePath(Vector2 fromPos, Vector2 toPos, PlayerTeam team, bool fullCheck = false) {
	int width = 4;
	float angle = TwoPointAngle(fromPos, toPos);
	Vector2 diff = toPos - fromPos;
	float offsetX = (float)Math.Cos(angle) * (width + 8);
	float offsetY = (float)Math.Sin(angle) * (width + 8);
	int itCount = (int)Math.Ceiling(diff.X / offsetX);
	Vector2 currentPos = fromPos;
	int vision = 0;
	string name = "";	
	for (int i = 0; i < itCount; i++) {
		Area area = new Area(currentPos.Y + width / 2.0f, currentPos.X - width / 2.0f, currentPos.Y - width / 2.0f, currentPos.X + width / 2.0f);
		IObject []objList = GlobalGame.GetObjectsByArea(area);		
		for (int j = 0; j < objList.Length; j++) {	
			name = objList[j].Name;
			if (name.StartsWith("Bg") || name.StartsWith("FarBg")) continue;
			if (IsPlayer(name)) {
				TPlayer pl = GetPlayer((IPlayer)objList[j]);
				if (pl.IsAlive() && !fullCheck) {
					if (pl.Team == team) vision = 3;
					else return vision;			
				} else vision = Math.Max(vision, 1);
			} else if (VisionObjects.ContainsKey(name)) vision = Math.Max(vision,VisionObjects[name]);
			if (vision >= 3) return vision;
		}
		currentPos.X += offsetX;
		currentPos.Y += offsetY;
	}	
	return vision;
}

public static bool CheckAreaToCollision(Area area) {
	string name = "";
	IObject []objList = GlobalGame.GetObjectsByArea(area);
	for (int j = 0; j < objList.Length; j++) {	
		name = objList[j].Name;
		if (name.StartsWith("Bg") || name.StartsWith("FarBg")) continue;
		if (VisionObjects.ContainsKey(name)) return true;
	}
	return false;
}

public static float TwoPointAngle(Vector2 beginPos, Vector2 endPos) {
	Vector2 diff = endPos - beginPos;
	return (float)Math.Atan2(diff.Y, diff.X);
}

public static List <IObject> GetTargetList(PlayerTeam team, Vector2 position, float distance, bool withTurrets, bool jammerProtection) {
	List <KeyValuePair <float, IObject> > targetList = new List <KeyValuePair <float, IObject> >();
	for (int i = 0; i < PlayerList.Count; i++) {
		if (PlayerList[i].IsAlive() && PlayerList[i].Team != team && (!PlayerList[i].Armor.Jammer || jammerProtection)) {
			IObject obj = PlayerList[i].User.GetPlayer();
			float dist = (position - obj.GetWorldPosition()).Length();
			if (dist > distance) continue;
			targetList.Add(new KeyValuePair <float, IObject>(dist, obj));
		}
	}
	if (withTurrets) {
		for (int i = 0; i < TurretList.Count; i++) {
			if (TurretList[i].MainMotor != null && TurretList[i].Team != team) {
				IObject obj = TurretList[i].MainMotor;
				float dist = (position - obj.GetWorldPosition()).Length();
				if (dist > distance) continue;
				targetList.Add(new KeyValuePair <float, IObject>(dist, obj));
			}
		}
	}
	targetList.Sort((x, y) => x.Key.CompareTo(y.Key));
	List <IObject> list = new List <IObject>();
	for (int i = 0; i < targetList.Count; i++) {
		list.Add(targetList[i].Value);
	}	
	return list;
}

public static List <IObject> GetFriendList(PlayerTeam team, Vector2 position, float distance) {
	List <KeyValuePair <float, IObject> > targetList = new List <KeyValuePair <float, IObject> >();
	for (int i = 0; i < PlayerList.Count; i++) {
		if (PlayerList[i].IsAlive() && PlayerList[i].Team == team) {
			IObject obj = PlayerList[i].User.GetPlayer();
			float dist = (position - obj.GetWorldPosition()).Length();
			if (dist > distance) continue;
			targetList.Add(new KeyValuePair <float, IObject>(dist, obj));
		}
	}
	targetList.Sort((x, y) => x.Key.CompareTo(y.Key));
	List <IObject> list = new List <IObject>();
	for (int i = 0; i < targetList.Count; i++) {
		list.Add(targetList[i].Value);
	}	
	return list;

}

public static float GetAngleDistance(float angle1, float angle2) {
	while (Math.Abs(angle1 - angle2) > Math.PI) {
		if (angle1 > angle2) angle1 -= (float)Math.PI * 2;
		else angle2 -= (float)Math.PI * 2;
	}
	return angle1 - angle2;
}

public static Vector2 GetPlayerCenter(IPlayer pl) {
	if (pl == null) return new Vector2(0 ,0);
	Vector2 pos = pl.GetWorldPosition();
	if (!pl.IsCrouching && !pl.IsDiving && !pl.IsFalling && !pl.IsLayingOnGround) pos.Y += 4;
	return pos;
}
public static void ApplyWeaponMod(TPlayer player, int id) {
	IPlayer pl = player.User.GetPlayer();
	switch (id) {
		case 1: {
			pl.GiveWeaponItem(WeaponItem.LAZER);
			break;
		} case 2: {
			WeaponItem first = pl.CurrentPrimaryWeapon.WeaponItem;
			WeaponItem second = pl.CurrentSecondaryWeapon.WeaponItem;
			if (ExtraAmmoWeapon.Contains(first)) pl.GiveWeaponItem(first);
			if (ExtraAmmoWeapon.Contains(second)) pl.GiveWeaponItem(second);
			break;
		} case 3: {
			WeaponItem first = pl.CurrentPrimaryWeapon.WeaponItem;
			WeaponItem second = pl.CurrentSecondaryWeapon.WeaponItem;
			WeaponItem thrown = pl.CurrentThrownItem.WeaponItem;
			if (ExtraExplosiveWeapon.Contains(first)) pl.GiveWeaponItem(first);
			if (ExtraExplosiveWeapon.Contains(second)) pl.GiveWeaponItem(second);
			if (ExtraExplosiveWeapon.Contains(thrown)) pl.GiveWeaponItem(thrown);
			break;
		} case 4: {
			WeaponItem first = pl.CurrentPrimaryWeapon.WeaponItem;
			WeaponItem second = pl.CurrentSecondaryWeapon.WeaponItem;
			if (ExtraHeavyAmmoWeapon.Contains(first)) pl.GiveWeaponItem(first);
			if (ExtraHeavyAmmoWeapon.Contains(second)) pl.GiveWeaponItem(second);
			break;
		} case 5: {			
			if (player.PrimaryWeapon != null) {
				player.PrimaryWeapon.DNAProtection = true;
				player.PrimaryWeapon.TeamDNA = player.Team;
			} else if (player.SecondaryWeapon != null) {
				player.SecondaryWeapon.DNAProtection = true;
				player.SecondaryWeapon.TeamDNA = player.Team;
			}
			break;
		} case 6: {
			pl.GiveWeaponItem(WeaponItem.BOUNCINGAMMO);
			break;
		}
	}
}

public static void ApplyThrownWeapon(TPlayer player, int id) {
	IPlayer pl = player.User.GetPlayer();
	switch (id) {
		case 1: {
			pl.GiveWeaponItem(WeaponItem.GRENADES);
			break;
		} case 2: {
			pl.GiveWeaponItem(WeaponItem.MOLOTOVS);
			break;
		} case 3: {
			pl.GiveWeaponItem(WeaponItem.MINES);
			break;
		} case 4: {
			pl.GiveWeaponItem(WeaponItem.GRENADES);
			player.WeaponTrackingUpdate(true);
			player.ThrownWeapon.CustomId = 1;
			break;
		} case 5: {
			pl.GiveWeaponItem(WeaponItem.GRENADES);
			player.WeaponTrackingUpdate(true);
			player.ThrownWeapon.CustomId = 2;
			break;
		} case 6: {
			pl.GiveWeaponItem(WeaponItem.GRENADES);
			player.WeaponTrackingUpdate(true);
			player.ThrownWeapon.CustomId = 3;
			break;
		} case 7: {
			pl.GiveWeaponItem(WeaponItem.SHURIKEN);
			break;
		}
	}
	if (id < 4) player.WeaponTrackingUpdate(true);
}

public static void ApplyWeapon(TPlayer player, WeaponItem id) {
	IPlayer pl = player.User.GetPlayer();
	pl.GiveWeaponItem(id);
}

public static bool IsJamming(PlayerTeam team) {
	for (int i = 0; i < TeamJamming.Length; ++i) {
		if (i == (int)team - 1) continue;
		else if (TeamJamming[i] > 0) return true;
	}
	return false;
}

public static bool IsHacking(PlayerTeam team) {
	for (int i = 0; i < TeamHacking.Length; ++i) {
		if (i == (int)team - 1) continue;
		else if (TeamHacking[i] > 0) return true;
	}
	return false;
}

public static string FormatName(string name) {
	name = name.Replace(";","_").Replace(":","_").Replace("\\","_").Replace("\'","_");
	return name;
}

public static void GenerateDroneMap() {
	for (int i = 0; i < DroneAreaSize.X; ++i) {
		List <int> line = new List <int>();
		for (int j = 0; j < DroneAreaSize.Y; ++j) {
			line.Add(0);
		}
		DroneMap1x1.Add(line);
	}	
	IObject []areaList = GlobalGame.GetObjectsByCustomId("Drone");
	for (int i = 0; i < areaList.Length; ++i) {
		Vector2 begin = areaList[i].GetWorldPosition() - DroneAreaBegin;
		begin.X = (int)begin.X / 8;
		begin.Y = (int)begin.Y / 8;
		for (float x = begin.X; x < begin.X + (float)areaList[i].GetSizeFactor().X; x += 1) {
			for (float y = begin.Y - (float)areaList[i].GetSizeFactor().Y + 1; y <= begin.Y; y += 1) {		
				DroneMap1x1[(int)x][(int)y] = 1;
			}
		}
	}
	for (int i = 0; i < DroneAreaSize.X; ++i) {
		for (int j = 0; j < DroneAreaSize.Y; ++j) {
			if (DroneMap1x1[i][j] > 0) {
				if (DroneMap1x1[i + 1][j] > 0 && DroneMap1x1[i + 1][j + 1] > 0 && DroneMap1x1[i][j + 1] > 0) DroneMap1x1[i][j] = 2;
				else if (DroneMap1x1[i + 1][j] > 0 && DroneMap1x1[i + 1][j - 1] > 0 && DroneMap1x1[i][j - 1] > 0) DroneMap1x1[i][j] = 2;
				else if (DroneMap1x1[i - 1][j] > 0 && DroneMap1x1[i - 1][j - 1] > 0 && DroneMap1x1[i][j - 1] > 0) DroneMap1x1[i][j] = 2;
				else if (DroneMap1x1[i - 1][j] > 0 && DroneMap1x1[i - 1][j + 1] > 0 && DroneMap1x1[i][j + 1] > 0) DroneMap1x1[i][j] = 2;
			}
		}
	}
}

public static Vector2 GetNearestDroneMapCell(Vector2 position, int size) {
	Vector2 center = position - DroneAreaBegin;
	center.X = (int)center.X / 8;
	center.Y = (int)center.Y / 8;
	if (DroneMap1x1[(int)center.X][(int)center.Y] >= size) return center;
	for (int i = 1; i < 100; ++i) {
		if (center.Y + i < DroneAreaSize.Y) {
			for (int j = Math.Max(0, (int)center.X - i); j <= Math.Min((int)center.X + i, DroneAreaSize.X - 1); ++j) {
				if (DroneMap1x1[(int)j][(int)center.Y + i] >= size) return new Vector2(j, center.Y + i);
			}
		}
		if (center.Y - i > 0) {
			for (int j = Math.Max(0, (int)center.X - i); j <= Math.Min((int)center.X + i, DroneAreaSize.X - 1); ++j) {
				if (DroneMap1x1[(int)j][(int)center.Y - i] >= size) return new Vector2(j, center.Y - i);
			}
		}
		if (center.X - i > 0) {
			for (int j = Math.Max(0, (int)center.Y - i + 1); j <= Math.Min((int)center.Y + i - 1, DroneAreaSize.Y - 1); ++j) {
				if (DroneMap1x1[(int)center.X - i][(int)j] >= size) return new Vector2(center.X - i, j);
			}
		}
		if (center.X + i < DroneAreaSize.X) {
			for (int j = Math.Max(0, (int)center.Y - i + 1); j <= Math.Min((int)center.Y + i - 1, DroneAreaSize.Y - 1); ++j) {
				if (DroneMap1x1[(int)center.X + i][(int)j] >= size) return new Vector2(center.X + i, j);
			}
		}
	}
	return center;
}

public static List<Vector2> FindDronePath(Vector2 from, Vector2 to, int size) {	
	List <Vector2> toCheck = new List <Vector2>();
	List <List <float> > pathMap = new List <List<float> >();
	for (int i = 0; i < DroneAreaSize.X; ++i) {
		List <float> line = new List <float>();
		for (int j = 0; j < DroneAreaSize.Y; ++j) {
			line.Add(1000);
		}
		pathMap.Add(line);
	}	
	pathMap[(int)from.X][(int)from.Y] = 0;
	toCheck.Add(from);
	while (toCheck.Count > 0) {		
		Vector2 current = toCheck[0];
		toCheck.RemoveAt(0);
		if ( pathMap[(int)current.X][(int)current.Y] >= pathMap[(int)to.X][(int)to.Y]) continue;
		if (current.X > 0 && DroneMap1x1[(int)current.X - 1][(int)current.Y] >= size && pathMap[(int)current.X - 1][(int)current.Y] > pathMap[(int)current.X][(int)current.Y] + 1) {
			pathMap[(int)current.X - 1][(int)current.Y] = pathMap[(int)current.X][(int)current.Y] + 1;
			toCheck.Add(new Vector2(current.X - 1, current.Y));
		}
		if (current.X + 1 < DroneAreaSize.X && DroneMap1x1[(int)current.X + 1][(int)current.Y] >= size && pathMap[(int)current.X + 1][(int)current.Y] > pathMap[(int)current.X][(int)current.Y] + 1) {
			pathMap[(int)current.X + 1][(int)current.Y] = pathMap[(int)current.X][(int)current.Y] + 1;
			toCheck.Add(new Vector2(current.X + 1, current.Y));
		} 
		if (current.Y > 0 && DroneMap1x1[(int)current.X][(int)current.Y - 1] >= size && pathMap[(int)current.X][(int)current.Y - 1] > pathMap[(int)current.X][(int)current.Y] + 1) {
			pathMap[(int)current.X][(int)current.Y - 1] = pathMap[(int)current.X][(int)current.Y] + 1;
			toCheck.Add(new Vector2(current.X, current.Y - 1));
		} 
		if (current.Y + 1 < DroneAreaSize.Y && DroneMap1x1[(int)current.X][(int)current.Y + 1] >= size && pathMap[(int)current.X][(int)current.Y + 1] > pathMap[(int)current.X][(int)current.Y] + 1) {
			pathMap[(int)current.X][(int)current.Y + 1] = pathMap[(int)current.X][(int)current.Y] + 1;
			toCheck.Add(new Vector2(current.X, current.Y + 1));
		}	

		if (current.X > 0 && current.Y > 0 &&  DroneMap1x1[(int)current.X - 1][(int)current.Y] >= size 
		&& DroneMap1x1[(int)current.X][(int)current.Y - 1] >= size 
		&& DroneMap1x1[(int)current.X - 1][(int)current.Y - 1] >= size 
		&& pathMap[(int)current.X - 1][(int)current.Y - 1] > pathMap[(int)current.X][(int)current.Y] + 1.4f) 
		{
			pathMap[(int)current.X - 1][(int)current.Y - 1] = pathMap[(int)current.X][(int)current.Y] + 1.4f;
			toCheck.Add(new Vector2(current.X - 1, current.Y - 1));
		}

		if (current.X > 0 && current.Y + 1 < DroneAreaSize.Y &&  DroneMap1x1[(int)current.X - 1][(int)current.Y] >= size 
		&& DroneMap1x1[(int)current.X][(int)current.Y + 1] >= size 
		&& DroneMap1x1[(int)current.X - 1][(int)current.Y + 1] >= size 
		&& pathMap[(int)current.X - 1][(int)current.Y + 1] > pathMap[(int)current.X][(int)current.Y] + 1.4f) 
		{
			pathMap[(int)current.X - 1][(int)current.Y + 1] = pathMap[(int)current.X][(int)current.Y] + 1.4f;
			toCheck.Add(new Vector2(current.X - 1, current.Y + 1));
		}	

		if (current.X + 1 < DroneAreaSize.X && current.Y > 0 &&  DroneMap1x1[(int)current.X + 1][(int)current.Y] >= size 
		&& DroneMap1x1[(int)current.X][(int)current.Y - 1] >= size 
		&& DroneMap1x1[(int)current.X + 1][(int)current.Y - 1] >= size 
		&& pathMap[(int)current.X + 1][(int)current.Y - 1] > pathMap[(int)current.X][(int)current.Y] + 1.4f) 
		{
			pathMap[(int)current.X + 1][(int)current.Y - 1] = pathMap[(int)current.X][(int)current.Y] + 1.4f;
			toCheck.Add(new Vector2(current.X + 1, current.Y - 1));
		}

		if (current.X + 1 < DroneAreaSize.X && current.Y + 1 < DroneAreaSize.Y &&  DroneMap1x1[(int)current.X + 1][(int)current.Y] >= size 
		&& DroneMap1x1[(int)current.X][(int)current.Y + 1] >= size 
		&& DroneMap1x1[(int)current.X + 1][(int)current.Y + 1] >= size 
		&& pathMap[(int)current.X + 1][(int)current.Y + 1] > pathMap[(int)current.X][(int)current.Y] + 1.4f) 
		{
			pathMap[(int)current.X + 1][(int)current.Y + 1] = pathMap[(int)current.X][(int)current.Y] + 1.4f;
			toCheck.Add(new Vector2(current.X + 1, current.Y + 1));
		}
	}
	List<Vector2> path = new List<Vector2>();
	if (pathMap[(int)to.X][(int)to.Y] == 1000) return path;	
	Vector2 next = to;
	path.Add(GetDroneMapPosition((int)to.X, (int)to.Y, size));	
	while (next != from) {
		Vector2 dir = new Vector2(0, 0);
		float dist = 1000;
		for (int i = -1; i <= 1; i++) {
			for (int j = -1; j <= 1; j++) {
				if (i == 0 && j == 0) continue;
				if (next.X + i > 0 && next.Y + j > 0 && next.X + i < DroneAreaSize.X && next.Y + j < DroneAreaSize.Y
				&& pathMap[(int)next.X + i][(int)next.Y + j] < dist)
				{
					dir.X = i;	
					dir.Y = j;
					dist = pathMap[(int)next.X + i][(int)next.Y + j];
				}
			}
		} 
		next += dir;
		Vector2 waypoint = GetDroneMapPosition((int)next.X, (int)next.Y, size);
		if (path.Count >= 2) {
			Vector2 last1 = path[path.Count - 1];
			Vector2 last2 = path[path.Count - 2];
			if ((last1.X == last2.X || last1.Y == last2.Y) && (last1.X == waypoint.X || last1.Y == waypoint.Y)) {
				path[path.Count - 1] = waypoint;
				continue;
			}
		}
		path.Add(waypoint);
	}	
	return path;
}

public static Vector2 GetDroneMapPosition(int x, int y, int size) {
	if (size == 2) {
		if (DroneMap1x1[x + 1][y] > 0 && DroneMap1x1[x + 1][y + 1] > 0 && DroneMap1x1[x][y + 1] > 0) return new Vector2(x * 8 + 8, y * 8 + 6) + DroneAreaBegin;
		else if (DroneMap1x1[x + 1][y] > 0 && DroneMap1x1[x + 1][y - 1] > 0 && DroneMap1x1[x][y - 1] > 0) return new Vector2(x * 8 + 8, y * 8) + DroneAreaBegin;
		else if (DroneMap1x1[x - 1][y] > 0 && DroneMap1x1[x - 1][y - 1] > 0 && DroneMap1x1[x][y - 1] > 0) return new Vector2(x * 8 - 2, y * 8) + DroneAreaBegin;
		else if (DroneMap1x1[x - 1][y] > 0 && DroneMap1x1[x - 1][y + 1] > 0 && DroneMap1x1[x][y + 1] > 0) return new Vector2(x * 8 - 2, y * 8 + 6) + DroneAreaBegin;
	}
	return new Vector2(x * 8, y * 8) + DroneAreaBegin;
}

public static void SpawnDrone(int id, PlayerTeam team) {
	Area area = GlobalGame.GetCameraArea();
	float x = GlobalRandom.Next((int)(area.Left + area.Width / 5), (int)(area.Right - area.Width / 5));
	float y = area.Top - 10;
	CreateTurret(id, new Vector2(x, y), 1, team);
	GlobalGame.PlayEffect("EXP", new Vector2(x, y));
	GlobalGame.PlaySound("Explosion",  new Vector2(x, y), 1.0f);
}

public static void ElectricExplosion(Vector2 position, int damage, float range) {
	for (int i = 0; i < PlayerList.Count; i++) {
		if (PlayerList[i].User.GetPlayer() == null) continue;
		float dist = (PlayerList[i].Position - position).Length();
		if (dist <= range) {
			PlayerList[i].Hp -= damage;
			CreateEffect(PlayerList[i].User.GetPlayer(), "S_P", 10, 10);			
		}
	}
}

public static void ThrowingUpdate() {
	IObject[] objects = GlobalGame.GetMissileObjects();	
	for (int i = 0; i < objects.Length; i++) {
		if (!AllowedMissile.Contains(objects[i].Name)) {
			objects[i].TrackAsMissile(false);
		}
	}
}

public static void StunExplosion(Vector2 position, float range, int duration) {
	for (int i = 0; i < PlayerList.Count; i++) {
		if (PlayerList[i].User.GetPlayer() != null && !PlayerList[i].User.GetPlayer().IsDead) {
			float dist = (PlayerList[i].Position - position).Length();
			if (dist <= range) {
				if (TracePath(position, PlayerList[i].Position, PlayerTeam.Independent, true) <= 2) {
					PlayerList[i].StunTime += duration;				
				}						
			}
		}
	}
}