using UnityEngine;
using System;
using System.Collections.Generic;

//1
[CreateAssetMenu(fileName ="LevelData", menuName = "PassingBy/LevelData")]
public class LevelData : ScriptableObject {

    //2
	public List<BattleEvent> battleData;
	public GameObject levelPrefab;
	public string levelName;
}

//the battle event, which will happen when the position.x of the gamemanager reaches column value
//enemies parameter defines all the enemies that will spawn
[Serializable]
public class BattleEvent {

    //3
	public int column;
    //4
	public List<EnemyData> enemies;
}

//5
//the enemy data containing the type of enemy, 
//the color of the enemy (if robot), 
//the row which it will spawn, 
//and the offset from the center 
[Serializable]
public class EnemyData {
	public EnemyType type;
	public RobotColor color;
	public int row;
	public float offset;
}

//the enumeration type to state whether the enemy is a robot or a boss
public enum EnemyType {
	Robot = 0,
	Boss
}