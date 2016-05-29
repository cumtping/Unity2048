using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
	public GameObject gameBoard;
	public GameObject block1Prefab, block2Prefab, block3Prefab, block4Prefab, block5Prefab,
					  block6Prefab, block7Prefab, block8Prefab, block9Prefab, block10Prefab,
					  block11Prefab, block12Prefab, block13Prefab, block14Prefab, block15Prefab,
					  block16Prefab, block17Prefab, block18Prefab, block19Prefab, block20Prefab;
	GameObject[] blockPrefabs;

	int xNum = 4, yNum = 4;
	int[,] blocks = new int[4, 4];//{{0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 0}}
	GameObject[,] blockObjs = new GameObject[4, 4];

	int blockSize = 240;
	int blockGap = 30;
	Vector2 block0Center;
	Vector2 mouseDownPos;
	Vector2 gameBoardCenter;

	void Start () {
		init ();
		spawnTwoBlock ();
	}

	void init() {
		// （0, 0）点中心距GameBoard正中心的位置
		int block0CenterX = -1 * (int)((blockSize + blockGap) * 3 / 2f);
		int block0CenterY = (int)(blockSize * 3 / 2f) + 2 * blockGap;
		block0Center = new Vector2 (block0CenterX, block0CenterY);

		gameBoardCenter = gameBoard.GetComponent<Transform> ().position;
		Debug.Log ("centerX=" + gameBoardCenter.x + ", centerY=" + gameBoardCenter.y);

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
				if (Mathf.Abs (moveX) > Mathf.Abs (moveY)) {
					if (moveX > 0) {
						//Debug.Log ("Right");
						moveBlocksRight ();
					} else {
						//Debug.Log ("Left");
						moveBlocksLeft ();
					}
				} else {
					if (moveY > 0) {
						//Debug.Log ("UP");
						moveBlocksUp ();
					} else {
						//Debug.Log ("Down");
						moveBlocksDown ();
					}
				}
				StartCoroutine (waitAndSpawnTwoBlock (1.5F));
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
			while (blocks [x, y] != 0) {
				x = Random.Range (0, xNum - 1);
				y = Random.Range (0, yNum - 1);
			}

			//Debug.Log ("spawnOneBlock x=" + x + " y=" + y);
			blocks [x, y] = 1;
			createOneBlock (x, y, 0);
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
		GameObject child = (GameObject)Instantiate (blockPrefabs[blockIndex], 
			new Vector3(blockPos.x, blockPos.y, 0), Quaternion.identity); 
		child.transform.SetParent(gameBoard.transform, false);
		blockObjs [x, y] = child;
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

	// 通过方块tab找到index
	int getBlockIndexByTag(string tag) {
		if (tag != null && tag.Contains ("Block_")) {
			string indexString = tag.Replace ("Block_", "");
			int index = int.Parse (indexString);
			return index - 1;
		}
		return -1;
	}

	// 判断游戏是否已结束（不能消除且没有剩余的空间）
	bool isGameOver() {
		bool gameOver = true;
		for (int i = 0; i < xNum; i++) {
			for (int j = 0; j < yNum; j++) {
				if (blocks [i, j] == 0) {
					gameOver = false;
					break;
				}
			}
			if (!gameOver) {
				break;
			}
		}
		Debug.Log ("gameOver=" + gameOver);
		return gameOver;
	}

	// 判断两个物体的tag是否一致
	bool haveSameTag(GameObject obj1, GameObject obj2){
		if (null == obj1 || null == obj2) {
			return false;
		}
		return obj1.tag != null && obj1.tag.Equals (obj2.tag);
	}

	// 所有方块下移
	void moveBlocksDown(){
		for (int x = 0; x < xNum; x++) {
			bool added = false;
			for (int y = yNum - 1; y >= 0; y--) {
				if (blocks [x, y] != 0) {
					// 上面的方块数字一致，相加；
					if (!added && y != 0 && haveSameTag(blockObjs[x, y], blockObjs[x, y - 1])) {
						added = true;

						int nextIndex = getBlockIndexByTag (blockObjs [x, y].tag) + 1;
						Destroy (blockObjs[x, y]);
						Destroy (blockObjs[x, y - 1]);
						createOneBlock (x, y, nextIndex);

						blockObjs [x, y - 1] = null;
						blocks [x, y - 1] = 0;
					}

					for (int i = yNum - 1; i > y; i--) {
						if (0 == blocks [x, i]) {
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
			bool added = false;
			for (int y = 0; y < yNum; y++) {
				if (blocks [x, y] != 0) {
					// 下面的方块数字一致，相加；
					if (!added && y != yNum - 1 && haveSameTag(blockObjs[x, y], blockObjs[x, y + 1])) {
						added = true;

						int nextIndex = getBlockIndexByTag (blockObjs [x, y].tag) + 1;
						Destroy (blockObjs[x, y]);
						Destroy (blockObjs[x, y + 1]);
						createOneBlock (x, y, nextIndex);

						blockObjs [x, y + 1] = null;
						blocks [x, y + 1] = 0;
					}

					for (int i = 0; i < y; i++) {
						if (0 == blocks [x, i]) {
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
			bool added = false;
			for (int x = xNum - 1; x >= 0; x--) {
				// 左面的方块数字一致，相加；
				if (!added && x != 0 && haveSameTag(blockObjs[x, y], blockObjs[x - 1, y])) {
					added = true;

					int nextIndex = getBlockIndexByTag (blockObjs [x, y].tag) + 1;
					Destroy (blockObjs[x, y]);
					Destroy (blockObjs[x - 1, y]);
					createOneBlock (x, y, nextIndex);

					blockObjs [x - 1, y] = null;
					blocks [x - 1, y] = 0;
				}

				if (blocks [x, y] != 0) {
					for (int i = xNum - 1; i > x; i--) {
						if (0 == blocks [i, y]) {
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
			bool added = false;
			for (int x = 0; x < xNum; x++) {
				if (blocks [x, y] != 0) {
					// 右面的方块数字一致，相加；(每一行只加一次)
					if (!added && x != xNum - 1 && haveSameTag(blockObjs[x, y], blockObjs[x + 1, y])) {
						added = true;

						int nextIndex = getBlockIndexByTag (blockObjs [x, y].tag) + 1;
						Destroy (blockObjs[x, y]);
						Destroy (blockObjs[x + 1, y]);
						createOneBlock (x, y, nextIndex);

						blockObjs [x + 1, y] = null;
						blocks [x + 1, y] = 0;
					}

					for (int i = 0; i < x; i++) {
						if (0 == blocks [i, y]) {
							moveOneBlock (x, y, i, y);
							break;
						}
					}
				}
			}
		}
	}

	// 移动一个方块
	void moveOneBlock(int xOld,int yOld, int xNew, int yNew){
		GameObject block = blockObjs [xOld, yOld];
		if (block != null) {
			Vector2 newPos = getBlockWorldPos (xNew, yNew);
			//Debug.Log ("xOld=" + xOld + ", yOld=" + yOld + ", xNew=" + xNew + 
			//	", yNew=" + yNew + ", newPosX=" + newPos.x + ", newPosY=" + newPos.y);
			iTween.MoveTo (block, iTween.Hash("y", newPos.y, "x", newPos.x, "delay", .2));	
		} else {
			Debug.Log ("moveOneBlockDown block is null !!! xOld=" + xOld + ", yOld=" + yOld + ", xNew=" + xNew + ", yNew=" + yNew);
		}
		blocks [xNew, yNew] = blocks [xOld, yOld];
		blocks [xOld, yOld] = 0;
		blockObjs [xNew, yNew] = blockObjs [xOld, yOld];
		blockObjs [xOld, yOld] = null;
	}
}
