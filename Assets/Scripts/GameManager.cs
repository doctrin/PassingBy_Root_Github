using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Com.LuisPedroFonseca.ProCamera2D;


public class GameManager : BasePC2D {

    public Hero actor;                 //Camera가 follow 할 Hero character
    public bool cameraFollows = true;  // Beat_Em_Up_Game_Starter_Kit 에서는 쓰지만, 우리는 ProCamera 2D가 알아서 해줌.
    public CameraBounds cameraBounds;

    public LevelData currentLevelData;
    private BattleEvent currentBattleEvent;
    private int nextEventIndex;
    public bool hasRemainingEvents;

    public List<GameObject> activeEnemies;
    public Transform[] spawnPositions;

    public GameObject currentLevelBackground;

    public GameObject robotPrefab;

    public Transform walkInStartTarget;
    public Transform walkInTarget;

    // Use this for initialization
    void Start () {
        /* //불필요한 카메라 위치 조정 로직 임시 배제
                if (cameraBounds)
                {
                    //cameraBounds.SetXPosition(cameraBounds.minVisibleX);
                }
        */

        //선비 캐릭터 x 좌표 임시 조정 - start -
        GameObject.Find("Sunbi").transform.position = new Vector3(GameObject.Find("Main Camera").transform.localPosition.x, 
                                                                  GameObject.Find("Sunbi").transform.position.y, 
                                                                  GameObject.Find("Sunbi").transform.position.z);
        //선비 캐릭터 x 좌표 임시 조정 - end -


        nextEventIndex = 0;
        StartCoroutine(LoadLevelData(currentLevelData));

    }

    // Update is called once per frame
    void Update () {

        //1
        if (currentBattleEvent == null && hasRemainingEvents)
        {
            if (Mathf.Abs(currentLevelData.battleData[nextEventIndex].column - cameraBounds.activeCamera.transform.position.x) > 0.2f)
            {
                PlayBattleEvent(currentLevelData.battleData[nextEventIndex]);
            }
        }

        //2
        if(currentBattleEvent != null)
        {
            //has event, check if enemies are alive
            if(Robot.TotalEnemies == 0)
            {
                //no more enemies;
                CompleteCurrentEvent();
            }
        }

        if (cameraFollows) {
            if (cameraBounds)
            {
                //cameraBounds.SetXPosition(actor.transform.position.x);
            }
        }

        //ProCamera2D의 바운더리를 LeftCamBounds, RightCamBounds 의 x 좌표로 계속 Update.
        ProCamera2D.GetComponent<ProCamera2DNumericBoundaries>().LeftBoundary = GameObject.Find("LeftCamBounds").transform.position.x; //좌측 바운더리에 접근 & 수정 가능!!
        ProCamera2D.GetComponent<ProCamera2DNumericBoundaries>().RightBoundary = GameObject.Find("RightCamBounds").transform.position.x; //우측 바운더리에 접근 & 수정 가능!!

    }

    private GameObject SpawnEnemy(EnemyData data)
    {
        GameObject enemyObj = Instantiate(robotPrefab);

        Vector3 position = spawnPositions[data.row].position;
        position.x = cameraBounds.activeCamera.transform.position.x + (data.offset * (cameraBounds.cameraHalfWidth + 1));
        enemyObj.transform.position = position;

        if(data.type == EnemyType.Robot)
        {
            enemyObj.GetComponent<Robot>().SetColor(data.color);
        }

        enemyObj.GetComponent<Enemy>().RegisterEnemy();

        return enemyObj;
    }

    private void PlayBattleEvent(BattleEvent battleEventData)
    {
        //현재 Battle event Get
        currentBattleEvent = battleEventData;
        nextEventIndex++;

        //카메라 Moving 관련된 부분인데, 우리 게임에 맞게 커스텀 할 필요 있음.
        cameraFollows = false;

        //<--- BattleEvent 객체당 정해진 column 이라는 항목이 있는데, column의 값으로 rootCamera의 x position을 변경한다.
        //그럴 필요는 없다.
        /* //불필요한 카메라 위치 조정 로직 임시 배제
                cameraBounds.SetXPosition(battleEventData.column); 
        */

        //Battle Event 관련 all 초기화 - Enemy제거, TotalEnemies 초기화
        foreach (GameObject enemy in activeEnemies)
        {
            Destroy(enemy);
        }
        activeEnemies.Clear();
        Enemy.TotalEnemies = 0;

        //TO-DO - START - TEST 시작
        //return; // 해당 return문에 주석을 제거 하면, Enemy가 양 사이드에서 출현하는 로직 실행
        //TO-DO - END

        //Enemy 생성
        foreach (EnemyData enemyData in currentBattleEvent.enemies)
        {
            activeEnemies.Add(SpawnEnemy(enemyData));
        }
    }

    private void CompleteCurrentEvent()
    {
        currentBattleEvent = null;

        cameraFollows = true;
        hasRemainingEvents = currentLevelData.battleData.Count > nextEventIndex;
    }

    private IEnumerator LoadLevelData(LevelData data)
    {
        //카메라 following을 멈추고, 현재 레벨 데이터를 획득
        cameraFollows    = false;
        currentLevelData = data;

        //획득한 레벨 데이터에서 배틀 이벤트가 있는지 확인 하고 획득
        hasRemainingEvents = currentLevelData.battleData.Count > 0;

        //activeEnemies 리스트 생성.
        activeEnemies      = new List<GameObject>();

        //1 프레임 pause
        yield return null;

        /* 
        //불필요한 카메라 위치 조정 로직 임시 배제
                //setting x axis to minVisibleX
                cameraBounds.SetXPosition(cameraBounds.minVisibleX);
        */

        //타일 맵 생성 from levelPreFab
        currentLevelBackground = Instantiate(currentLevelData.levelPrefab);
        //actor(hero)를 walkin start 지점으로 위치이동.
        cameraBounds.EnableBounds(false);
        actor.transform.position = walkInStartTarget.transform.position;

        yield return new WaitForSeconds(0.1f);

        actor.UseAutopilot(true);
        actor.AnimateTo(walkInTarget.transform.position, false, DidFinishIntro);

        //레벨이 완전히 로딩이 되면, 다시 카메라 following 재개
        cameraFollows = true;

    }

    private void DidFinishIntro()
    {
        actor.UseAutopilot(false);
        actor.controllable = true;
        cameraBounds.EnableBounds(true);
    }

}
