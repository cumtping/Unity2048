using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
	bool testMode = true;
	bool isDebug = true;
	float spawnDelay = 1.0F;
	int xNum = 4, yNum = 4;
	int blockSize = 240;
	int blockGap = 30;
	string gameStatePrefName = "game_state";

	public GameObject gameBoard;
	public GameObject block1Prefab, block2Prefab, block3Prefab, block4Prefab, block5Prefab,
					  block6Prefab, block7Prefab, block8Prefab, block9Prefab, block10Prefab,
					  block11Prefab, block12Prefab, block13Prefab, block14Prefab, block15Prefab,
					  block16Prefab, block17Prefab, block18Prefab, block19Prefab, block20Prefab;
	GameObject[] blockPrefabs;
	// block索引；0代表空；1~20代表相应的数字block
	int[,] blockIndexArray = new int[4, 4];
	// block 游戏对象
	GameObject[,] blockObjArray = new GameObject[4, 4];

	Vector2 block0Center;
	Vector2 mouseDownPos;
	Vector2 gameBoardCenter;
	// block发生移动或相加
	bool blockMovedOrAdded;
	/// <优化随机位置生成>
	/// 0、Random方法设置seed；
	/// 1、将block从上到下、从左到右编码，依次是1~16；
	/// 2、每次移动完成，记录当前空闲的位置；
	/// 3，在当前空闲的位置随机生成新的；
	ArrayList emptyBlockList = new ArrayList();
	/// </优化随机位置生成>

	void Start () {
		init ();

		if (testMode) {
			spawnTestBlock ();	
		} else {
			if (!restoreGameState ()) {
				spawnBlock (2);
			}
		}
	}

	void Update () {
		if (Input.GetKey(KeyCode.Escape)) {
			storeGameState ();
			Application.Quit();
		}
		dealWithTouchvent ();
	}

	// 保存游戏状态
	void storeGameState (){
		string indexString = "";
		for (int x = 0; x < xNum; x++) {
			for (int y = 0; y < yNum; y++) {
				indexString += blockIndexArray [x, y] + " ";
			}
		}
		PlayerPrefs.SetString (gameStatePrefName, indexString.Trim ());
	}

	// 恢复游戏状态
	// return true：恢复成功；false：不需要恢复
	bool restoreGameState(){
		string indexString = PlayerPrefs.GetString (gameStatePrefName, null);

		if (indexString != null && !indexString.Equals("")) {
			string[] indexs = indexString.Split (' ');
			for (int i = 0; i < indexs.Length; i++) {
				int blockIndex = int.Parse (indexs[i]);
				if (blockIndex != 0) {
					Vector2 position = indexToPosition (i + 1);
					createOneBlock ((int)position.x, (int)position.y, blockIndex);
				}
			}
			return true;	
		} else {
			return false;
		}
	}

	// 重新开始游戏
	public void restartGame() {
		for (int i = 0; i < xNum; i++) {
			for (int j = 0; j < yNum; j++) {
				if (blockObjArray [i, j] != null) {
					GameObject.Destroy (blockObjArray[i, j]);
				}
			}
		}

		blockObjArray = new GameObject[4, 4];
		blockIndexArray = new int[4, 4];
		blockMovedOrAdded = false;

		// 生成两个方块
		spawnBlock (2);
		fillEmptyBlockList ();
	}

	/// <test methods>
	void spawnTestBlock () {
		bool testFullHorizontal = false;
		bool testFullVertical = false;
		bool testArray = true;
		int exception1 = -1;
		int exception2 = 2;

		if (testFullHorizontal) {
			for (int i = 0; i < xNum; i++) {
				if (i != exception1 && i != exception2) {
					createOneBlock (i, 1, 1);
				}
			}
		}
		if (testFullVertical) {
			for (int i = 0; i < yNum; i++) {
				if (i != exception1 && i != exception2) {
					createOneBlock (1, i, 1);
				}
			}
		}
		if (testArray) {
			int[,] indexArray = new int[4, 4]{ { 1, 2, 3, 4 }, { 5, 6, 7, 8 }, { 9, 11, 12, 13 }, { 14, 15, 16, 0 } };
			initGameBoard (indexArray);
		}
	}
	/// </test methods>

	void initGameBoard(int[,] indexArray){
		for (int i = 0; i < xNum; i++) {
			for (int j = 0; j < yNum; j++) {
				if (indexArray [i, j] != 0) {
					createOneBlock (i, j, indexArray [i, j]);
				}
			}
		}
	}

	void init() {
		// （0, 0）点中心距GameBoard正中心的位置
		int block0CenterX = -1 * (int)((blockSize + blockGap) * 3 / 2f);
		int block0CenterY = (int)(blockSize * 3 / 2f) + 2 * blockGap;
		block0Center = new Vector2 (block0CenterX, block0CenterY);

		gameBoardCenter = gameBoard.GetComponent<Transform> ().position;

		blockPrefabs = new GameObject[]{block1Prefab, block2Prefab, block3Prefab, block4Prefab, block5Prefab,
			block6Prefab, block7Prefab, block8Prefab, block9Prefab, block10Prefab,
			block11Prefab, block12Prefab, block13Prefab, block14Prefab, block15Prefab,
			block16Prefab, block17Prefab, block18Prefab, block19Prefab, block20Prefab};

		fillEmptyBlockList ();
	}

	// 处理滑动事件
	void dealWithTouchvent(){
		if (Input.GetMouseButtonDown (0)) {
			mouseDownPos = Input.mousePosition;
		} else if (Input.GetMouseButtonUp(0)) {
			Vector2 upPos = Input.mousePosition;
			float moveX = upPos.x - mouseDownPos.x;
			float moveY = upPos.y - mouseDownPos.y;
			if (Mathf.Abs (moveX) > 10 && Mathf.Abs (moveY) > 10) {
				blockMovedOrAdded = false;
				if (isDebug) Debug.Log ("=========START===================================");

				if (Mathf.Abs (moveX) > Mathf.Abs (moveY)) {
					if (moveX > 0) {
						if(isDebug) Debug.Log ("Right");
						moveBlocksRight ();
					} else {
						if(isDebug) Debug.Log ("Left");
						moveBlocksLeft ();
					}
				} else {
					if (moveY > 0) {
						if(isDebug) Debug.Log ("UP");
						moveBlocksUp ();
					} else {
						if(isDebug) Debug.Log ("Down");
						moveBlocksDown ();
					}
				}
				fillEmptyBlockList ();

				if (blockMovedOrAdded) {
					StartCoroutine (waitAndSpawnBlock (spawnDelay));
				}

				if (isDebug) Debug.Log ("=========END===================================");
			}
		}
	}

	void showGameOverDialog(){
		if (isDebug) Debug.Log ("================ Game Over ==============");
	}

	IEnumerator waitAndSpawnBlock(float waitTime){
		yield return new WaitForSeconds(waitTime);  
		spawnBlock (1);
		if (isGameOver ()) {
			showGameOverDialog ();	
		}
	}

	// 将空白方块编号存入emptyBlockList
	void fillEmptyBlockList(){
		emptyBlockList.Clear ();
		for (int x = 0; x < xNum; x++) {
			for (int y = 0; y < yNum; y++) {
				if (0 == blockIndexArray [x, y]) {
					emptyBlockList.Add (x * xNum + y + 1);
				}
			}
		}
	}

	// 从空白方块中随机选定一个
	int pickOneEmptyBlock(){
		if (emptyBlockList.Count == 0) {
			return 0;
		} else {
			int x = Random.Range (0, emptyBlockList.Count - 1);
			int index = (int)emptyBlockList [x];
			emptyBlockList.RemoveAt (x);
			return index;
		}
	}

	// 空白方块对应坐标
	Vector2 indexToPosition(int index){
		if (0 == index) {
			return new Vector2(-1, -1);
		} else {
			return new Vector2 ((index - 1) / xNum, (index - 1) % xNum);
		}
	}

	// 生成标2的方格，若成功生成返回true；若没有空间返回false;
	bool spawnBlock(int num) {
		for (int i = 1; i <= num; i++) {
			Vector2 position = indexToPosition (pickOneEmptyBlock ());
			if ((int)position.x != -1) {
				createOneBlock ((int)position.x, (int)position.y, 1);
			} else {
				if (isDebug) Debug.Log ("======================= no empty position =================");
			}
		}
		return true;
	}

	// 在第x行y列创建一个方格
	void createOneBlock(int x, int y, int blockIndex) {
		if (blockIndex < 1 || blockIndex > blockPrefabs.Length || null == blockPrefabs[blockIndex]) {
			Debug.Log ("createOneBlock, blockIndex out of bound!");
			return;
		}

		Vector2 blockPos = getBlockPos (x, y);
		GameObject child = (GameObject)Instantiate (blockPrefabs[blockIndex - 1], 
			new Vector3(blockPos.x, blockPos.y, 0), Quaternion.identity); 
		child.transform.SetParent(gameBoard.transform, false);
		blockObjArray [x, y] = child;
		blockIndexArray [x, y] = blockIndex;
	}

	// 获得某个block在游戏面板中的坐标位置
	Vector2 getBlockPos(int x, int y) {
		int xPos = (int)block0Center.x + x * (blockSize + blockGap);
		int yPos = (int)block0Center.y - y * (blockSize + blockGap);

		return new Vector2 (xPos, yPos);
	}

	// 获得某个block的世界坐标位置
	Vector2 getBlockWorldPos(int x, int y) {
		Vector2 newPos = getBlockPos (x, y);
		return new Vector2 (newPos.x + gameBoardCenter.x, newPos.y + gameBoardCenter.y);
	}

	// 判断游戏是否已结束（不能消除且没有剩余的空间）
	bool isGameOver() {
		bool gameOver = true;
		if (emptyBlockList.Count > 0) {
			gameOver = false;
		} else {
			for (int i = 0; i < xNum; i++) {
				for (int j = 0; j < yNum; j++) {
					if ((j != yNum - 1 && blockIndexArray [i, j] == blockIndexArray [i, j + 1]) ||
					   (j != 0 && blockIndexArray [i, j] == blockIndexArray [i, j - 1]) ||
					   (i != xNum - 1 && blockIndexArray [i, j] == blockIndexArray [i + 1, j]) ||
					   (i != 0 && blockIndexArray [i, j] == blockIndexArray [i - 1, j])) {
						gameOver = false;
						break;
					}
				}
				if (!gameOver) {
					break;
				}
			}
		}

		return gameOver;
	}

	// 所有方块下移
	void moveBlocksDown(){
		for (int x = 0; x < xNum; x++) {
			for (int y = yNum - 1; y >= 0; y--) {
				if (blockIndexArray [x, y] != 0) {
					// 与左面第一个不为空的方块数字一致，相加；(每一行只加一次)
					for (int i = y - 1; i >= 0; i--) {
						if (0 == blockIndexArray [x, i]) {
							continue;
						}
						bool added = addTwoBlock (x, y, x, i);
						if (added) {
							y = i - 1;
						}
						break;
					}
				}
			}
			for (int y = yNum - 2; y >= 0; y--) {
				if (blockIndexArray [x, y] != 0) {
					for (int i = yNum - 1; i > y; i--) {
						if (0 == blockIndexArray [x, i]) {
							moveOneBlock (x, y, x, i);
							break;
						}	
					}
				}
			}
		}
	}

	// 所有方块上移
	void moveBlocksUp(){
		for (int x = 0; x < xNum; x++) {
			for (int y = 0; y < yNum; y++) {
				if (blockIndexArray [x, y] != 0) {
					// 与下面第一个不为空的方块数字一致，相加；(每一列只加一次)
					for (int i = y + 1; i < yNum; i++) {
						if (0 == blockIndexArray [x, i]) {
							continue;
						}
						bool added = addTwoBlock (x, y, x, i);
						if (added) {
							y = i + 1;
						}
						break;
					}
				}
			}
			for (int y = 1; y < yNum; y++) {
				if (blockIndexArray [x, y] != 0) {
					for (int i = 0; i < y; i++) {
						if (0 == blockIndexArray [x, i]) {
							moveOneBlock (x, y, x, i);
							break;
						}	
					}
				}
			}
		}
	}

	// 所有方块右移
	void moveBlocksRight(){
		for (int y = 0; y < yNum; y++) {
			for (int x = xNum - 1; x >= 0; x--) {
				if (blockIndexArray [x, y] != 0) {
					// 与左面第一个不为空的方块数字一致，相加；(每一行只加一次)
					for (int i = x - 1; i >= 0; i--) {
						if (0 == blockIndexArray [i, y]) {
							continue;
						}
						bool added = addTwoBlock (x, y, i, y);
						if (added) {
							x = i - 1;
						}
						break;
					}
				}
			}

			for (int x = xNum - 2; x >= 0; x--) {
				if (blockIndexArray [x, y] != 0) {
					for (int i = xNum - 1; i > x; i--) {
						if (0 == blockIndexArray [i, y]) {
							moveOneBlock (x, y, i, y);
							break;
						}	
					}
				}
			}
		}
	}

	// 所有方块左移
	void moveBlocksLeft(){
		for (int y = 0; y < yNum; y++) {
			for (int x = 0; x < xNum; x++) {
				if (blockIndexArray [x, y] != 0) {
					// 与右面第一个不为空的方块数字一致，相加；(每一行只加一次)
					for (int i = x + 1; i < xNum; i++) {
						if (0 == blockIndexArray [i, y]) {
							continue;
						}
						bool added = addTwoBlock (x, y, i, y);
						if (added) {
							x = i + 1;
						}
						break;
					}
				}
			}

			for (int x = 1; x < xNum; x++) {
				if (blockIndexArray [x, y] != 0) {
					for (int i = 0; i < x; i++) {
						if (0 == blockIndexArray [i, y]) {
							moveOneBlock (x, y, i, y);
							break;
						}
					}
				}
			}
		}
	}

	// 将两个数字相同、非空且位置不同的block相加，前者保留，后者销毁
	// true：成功相加；false：未相加；
	bool addTwoBlock(int toX, int toY, int fromX, int fromY){
		if (blockIndexArray [toX, toY] != 0 && !(toX == fromX && toY == fromY) &&
			blockIndexArray [toX, toY] == blockIndexArray [fromX, fromY]) {
			if(isDebug) Debug.Log ("before add: addTwoBlock [" + toX + ", " + toY + "]=" + blockIndexArray [toX, toY]
				+ " and [" + fromX + ", " + fromY + "]=" + blockIndexArray [fromX, fromY]);
			
			int nextIndex = blockIndexArray [toX, toY] + 1;
			// 销毁原来的block
			Destroy (blockObjArray [toX, toY]);
			Destroy (blockObjArray [fromX, fromY]);
			blockObjArray [fromX, fromY] = null;
			blockIndexArray [fromX, fromY] = 0;
			// 创建新的block
			createOneBlock (toX, toY, nextIndex);

			blockMovedOrAdded = true;
			if(isDebug) Debug.Log ("after add: addTwoBlock [" + toX + ", " + toY + "]=" + blockIndexArray [toX, toY]
				+ " and [" + fromX + ", " + fromY + "]=" + blockIndexArray [fromX, fromY]);
			return true;
		} else {
			return false;
		}
	}

	// 移动一个方块
	void moveOneBlock(int xOld,int yOld, int xNew, int yNew){
		if(isDebug) Debug.Log ("before move: [" + xOld + ", " + yOld + "]=" + blockIndexArray [xOld, yOld]
			+ " and [" + xNew + ", " + yNew + "]=" + blockIndexArray [xNew, yNew]);
		GameObject block = blockObjArray [xOld, yOld];
		if (block != null) {
			Vector2 newPos = getBlockWorldPos (xNew, yNew);
			iTween.MoveTo (block, iTween.Hash("y", newPos.y, "x", newPos.x, "delay", .2));	
		} else {
			Debug.Log ("moveOneBlockDown block is null !!! xOld=" + xOld + ", yOld=" + yOld + ", xNew=" + xNew + ", yNew=" + yNew);
		}
		blockIndexArray [xNew, yNew] = blockIndexArray [xOld, yOld];
		blockIndexArray [xOld, yOld] = 0;
		blockObjArray [xNew, yNew] = blockObjArray [xOld, yOld];
		blockObjArray [xOld, yOld] = null;

		blockMovedOrAdded = true;
		if(isDebug) Debug.Log ("after move: [" + xOld + ", " + yOld + "]=" + blockIndexArray [xOld, yOld]
			+ " and [" + xNew + ", " + yNew + "]=" + blockIndexArray [xNew, yNew]);
	}


}
