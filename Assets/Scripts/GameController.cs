using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
	bool testMode = false;
	bool isDebug = false;
	float spawnDelay = 1.0F;
	int xNum = 4, yNum = 4;
	int blockSize = 240;
	int blockGap = 30;

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

	void Start () {
		init ();
		if (!testMode) {
			spawnTwoBlock ();
		} else {
			spawnTestBlock ();	
		}
	}

	/// <test methods>
	void spawnTestBlock () {
		bool testFullHorizontal = false;
		bool testFullVertical = true;
		int exception1 = 1;
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
	}
	/// </test methods>

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
	}

	void Update () {
		dealWithTouchvent ();
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
				if (!testMode && blockMovedOrAdded) {
					StartCoroutine (waitAndSpawnTwoBlock (spawnDelay));
				}
			}
		}
	}

	IEnumerator waitAndSpawnTwoBlock(float waitTime){
		yield return new WaitForSeconds(waitTime);  
		spawnTwoBlock ();
	}

	// 生成标2的方格，若成功生成返回true；若没有空间返回false;
	bool spawnTwoBlock() {
		for (int i = 1; i <= 2; i++) {
			if (isGameOver ()) {
				return false;
			}
			int x = Random.Range (0, xNum - 1);
			int y = Random.Range (0, yNum - 1);
			while (blockIndexArray [x, y] != 0) {
				x = Random.Range (0, xNum - 1);
				y = Random.Range (0, yNum - 1);
			}

			createOneBlock (x, y, 1);
		}
		return true;
	}

	// 在第x行y列创建一个方格
	void createOneBlock(int x, int y, int blockIndex) {
		if (blockIndex < 0 || blockIndex > blockPrefabs.Length || null == blockPrefabs[blockIndex]) {
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
		for (int i = 0; i < xNum; i++) {
			for (int j = 0; j < yNum; j++) {
				if (blockIndexArray [i, j] == 0) {
					gameOver = false;
					break;
				}
			}
			if (!gameOver) {
				break;
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
					}
				}
			}
			for (int y = yNum - 1; y >= 0; y--) {
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
					for (int i = y + 1; y < yNum; i++) {
						if (0 == blockIndexArray [x, i]) {
							continue;
						}
						bool added = addTwoBlock (x, y, x, i);
						if (added) {
							y = i + 1;
						}
					}
				}
			}
			for (int y = 0; y < yNum; y++) {
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
					}
				}
			}

			for (int x = xNum - 1; x >= 0; x--) {
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
				if (y == 1) {
					if(isDebug) Debug.Log ("before move: blockIndexArray[" + x + ", " + y + "] = " + blockIndexArray [x, y]);
				}
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
					}
				}
				if (y == 1) {
					if(isDebug) Debug.Log ("after move: blockIndexArray[" + x + ", " + y + "] = " + blockIndexArray [x, y]);
				}
			}

			for (int x = 1; x < xNum; x++) {
				if (blockIndexArray [x, y] != null) {
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

	// 将两个数字相同的非空block相加，前者保留，后者销毁
	// true：成功相加；false：未相加；
	bool addTwoBlock(int toX, int toY, int fromX, int fromY){
		if(isDebug) Debug.Log ("before add: addTwoBlock [" + toX + ", " + toY + "]=" + blockIndexArray [toX, toY]
			+ " and [" + fromX + ", " + fromY + "]=" + blockIndexArray [fromX, fromY]);
		if (blockIndexArray [toX, toY] != 0 && blockIndexArray [toX, toY] == blockIndexArray [fromX, fromY]) {
			int nextIndex = blockIndexArray [toX, toY] + 1;
			// 销毁原来的block
			Destroy (blockObjArray [toX, toY]);
			Destroy (blockObjArray [fromX, fromY]);
			blockObjArray [fromX, fromY] = null;
			blockIndexArray [fromX, fromY] = 0;
			// 创建新的block
			createOneBlock (toX, toY, nextIndex);

			blockMovedOrAdded = true;
			return true;
		} else {
			return false;
		}
		if(isDebug) Debug.Log ("after add: addTwoBlock [" + toX + ", " + toY + "]=" + blockIndexArray [toX, toY]
			+ " and [" + fromX + ", " + fromY + "]=" + blockIndexArray [fromX, fromY]);
	}

	// 移动一个方块
	void moveOneBlock(int xOld,int yOld, int xNew, int yNew){
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
	}
}
