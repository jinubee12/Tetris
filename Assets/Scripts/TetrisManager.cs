using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TetrisManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static TetrisManager Instance { get; private set; }
    
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI terminalText; // 터미널 역할을 할 UI Text
    [SerializeField] private TextMeshProUGUI _nextBlockText;

    [Header("Tetris Board Settings")]
    [SerializeField] private int width = 10;  // 가로 크기
    [SerializeField] private int height = 20; // 세로 크기
    
    [SerializeField] private AudioClip _beepSound;
    private AudioSource _audioSource;

    // 내부에서 사용할 게임판 데이터
    private char[,] board;

    // 현재 조작 중인 블록 정보
    private int currentX, currentY;
    private int[,] currentBlock;
    
    // 다음 블럭
    private int _nextBlockIndex;
    private int[,] _nextBlock;

    // 시간 계산용
    private float dropTimer = 0f;      // 자동 낙하 타이머
    private float dropInterval = 1f;   // 자동 낙하 간격 (초)

    // 블록 정의 (간단 예시)
    private int[,,] tetrominoShapes = new int[7, 4, 4]
    {
        // I
        {
            {0,0,1,0},
            {0,0,1,0},
            {0,0,1,0},
            {0,0,1,0}
        },
        // O
        {
            {0,0,0,0},
            {0,1,1,0},
            {0,1,1,0},
            {0,0,0,0}
        },
        // T
        {
            {0,0,0,0},
            {0,1,0,0},
            {1,1,1,0},
            {0,0,0,0}
        },
        // S
        {
            {0,0,0,0},
            {0,1,1,0},
            {1,1,0,0},
            {0,0,0,0}
        },
        // Z
        {
            {0,0,0,0},
            {0,1,1,0},
            {0,0,1,1},
            {0,0,0,0}
        },
        // J
        {
            {0,0,0,0},
            {0,1,0,0},
            {0,1,1,1},
            {0,0,0,0}
        },
        // L
        {
            {0,0,0,0},
            {0,0,1,0},
            {1,1,1,0},
            {0,0,0,0}
        }
    };

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        // 싱글톤 중복 방지
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    
    private void Start()
    {
        InitializeBoard();  // 게임판 초기화
        
        _nextBlockIndex = Random.Range(0, tetrominoShapes.GetLength(0)); // 첫번째 생성할 블록
        SpawnBlock();       // 블록 소환
        GenerateNextBlock();// 다음 블록 생성
        
        RenderBoard();      // 초기 화면 렌더링
        RenderNextBlock();  // 다음 블록 화면 렌더링
    }

    private void Update()
    {
        // 자동 낙하
        dropTimer += Time.deltaTime;
        if (dropTimer >= dropInterval)
        {
            DropBlock();
            dropTimer = 0f;
        }

        // 좌우 & 아래 이동
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveBlock(-1, 0);
            SoundManager.instance.PlayClip(_beepSound,_audioSource);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveBlock(1, 0);
            SoundManager.instance.PlayClip(_beepSound,_audioSource);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveBlock(0, 1);
            SoundManager.instance.PlayClip(_beepSound,_audioSource);
        }

        // 회전
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            RotateBlock();
            SoundManager.instance.PlayClip(_beepSound,_audioSource);
        }
    }

    /// <summary>
    /// 게임판(2D char 배열) 초기화
    /// </summary>
    private void InitializeBoard()
    {
        board = new char[height, width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                board[y, x] = '.'; // 빈 공간은 '.' 으로
            }
        }
    }

    /// <summary>
    /// 새 블록을 랜덤으로 스폰
    /// </summary>
    private void SpawnBlock()
    {
        int randomIndex = _nextBlockIndex;
        
        currentBlock = new int[4, 4];

        // 블록 데이터 복사
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                currentBlock[y, x] = tetrominoShapes[randomIndex, y, x];
            }
        }

        // 시작 위치 (가운데에서 생성)
        currentX = width / 2 - 2;
        currentY = 0;

        // 스폰 직후 충돌이 있다면 게임 오버 처리 가능
        if (CheckCollision(currentX, currentY))
        {
            // 간단히 전체 보드 초기화로 처리(혹은 게임 오버 로직 등)
            InitializeBoard();
        }
    }

    private void GenerateNextBlock()
    {
        _nextBlockIndex = Random.Range(0, tetrominoShapes.GetLength(0));
        _nextBlock = new int[4, 4];
        
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                _nextBlock[y, x] = tetrominoShapes[_nextBlockIndex, y, x];
            }
        }
    }

    /// <summary>
    /// 블록을 dx, dy 만큼 이동
    /// </summary>
    private void MoveBlock(int dx, int dy)
    {
        int newX = currentX + dx;
        int newY = currentY + dy;

        // 이동 전 충돌 체크
        if (!CheckCollision(newX, newY))
        {
            currentX = newX;
            currentY = newY;
        }
        else
        {
            // 아래로 이동 불가(충돌) 시, 블록 고정
            if (dy > 0) 
            {
                FixBlock();
            }
        }

        RenderBoard();
    }

    /// <summary>
    /// 블록을 한 칸 아래로 떨어뜨림(자동 낙하)
    /// </summary>
    private void DropBlock()
    {
        MoveBlock(0, 1);
    }

    /// <summary>
    /// 블록 회전 (90도 시계 방향)
    /// </summary>
    private void RotateBlock()
    {
        // 새로운 배열에 회전 결과를 임시 저장
        int[,] rotated = new int[4, 4];
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                rotated[x, 3 - y] = currentBlock[y, x];
            }
        }

        // 충돌 검사
        int oldX = currentX;
        int oldY = currentY;

        // 임시로 currentBlock을 회전된 형태로 바꿔서 확인
        currentBlock = rotated;
        if (CheckCollision(currentX, currentY))
        {
            // 충돌 발생 시 회전을 취소
            currentBlock = RotateBack(rotated);
        }

        RenderBoard();
    }

    /// <summary>
    /// 회전 취소용(역방향 회전)
    /// </summary>
    private int[,] RotateBack(int[,] rotated)
    {
        int[,] original = new int[4, 4];
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                // rotated[x, 3 - y] == 원래 currentBlock[y, x]
                // => 역으로 회전
                original[y, x] = rotated[3 - x, y];
            }
        }
        return original;
    }

    /// <summary>
    /// 블록이 보드 밖을 벗어나거나 다른 블록과 겹치는지 검사
    /// </summary>
    private bool CheckCollision(int newX, int newY)
    {
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                if (currentBlock[y, x] == 1)
                {
                    int boardX = newX + x;
                    int boardY = newY + y;

                    // 보드 범위를 벗어나거나 이미 '#'(고정 블록)이면 충돌
                    if (boardX < 0 || boardX >= width || boardY < 0 || boardY >= height)
                    {
                        return true;
                    }
                    if (board[boardY, boardX] == '#')
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 현재 블록을 보드에 고정(#)하고 새로운 블록 스폰
    /// </summary>
    private void FixBlock()
    {
        // 보드에 현재 블록을 '#'로 표시
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                if (currentBlock[y, x] == 1)
                {
                    int boardX = currentX + x;
                    int boardY = currentY + y;
                    board[boardY, boardX] = '#';
                }
            }
        }

        // 라인 클리어
        ClearLines();

        // 새 블록 스폰
        
        SpawnBlock();
        GenerateNextBlock();
        RenderNextBlock();
    }

    /// <summary>
    /// 한 줄이 전부 '#'이면 제거하고 위를 당겨옴
    /// </summary>
    private void ClearLines()
    {
        for (int y = 0; y < height; y++)
        {
            bool isLineFull = true;
            for (int x = 0; x < width; x++)
            {
                if (board[y, x] == '.')
                {
                    isLineFull = false;
                    break;
                }
            }
            if (isLineFull)
            {
                // y줄 위로 당기기
                for (int row = y; row > 0; row--)
                {
                    for (int col = 0; col < width; col++)
                    {
                        board[row, col] = board[row - 1, col];
                    }
                }
                // 가장 윗줄은 빈 줄로
                for (int col = 0; col < width; col++)
                {
                    board[0, col] = '.';
                }
            }
        }
    }

    /// <summary>
    /// 보드 상태를 터미널(Text UI)에 표시
    /// </summary>
    private void RenderBoard()
    {
        char[,] displayBoard = new char[height, width];
    
        // 1) 보드 복사
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                displayBoard[y, x] = board[y, x];
            }
        }

        // 2) 현재 블록 표시
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                if (currentBlock[y, x] == 1)
                {
                    int boardX = currentX + x;
                    int boardY = currentY + y;
                    if (boardY >= 0 && boardY < height && boardX >= 0 && boardX < width)
                    {
                        displayBoard[boardY, boardX] = 'O';
                    }
                }
            }
        }

        // 3) 문자열로 변환
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int y = 0; y < height; y++)
        {
            // 줄 시작: <! 추가
            sb.Append("<!");

            // 보드 데이터 출력
            for (int x = 0; x < width; x++)
            {
                sb.Append(" ");
                sb.Append(displayBoard[y, x]);
            }
        
            // 줄 끝: !> + 줄바꿈
            sb.Append("!>\n");
        }

        sb.Append("<!====================!>\n");
        sb.Append(@"\/\/\/\/\/\/\/\/\/\/"); //어쩌피 가운데 정렬
        sb.Append("\n");

        // 4) UI에 적용
        terminalText.text = sb.ToString();
    }

    private void RenderNextBlock()
    {
        char[,] _nextBlockScreen = new char[4, 4];
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                _nextBlockScreen[y, x] = ' '; // 빈 공간은 '.' 으로
            }
        }
        
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                if (_nextBlock[y, x] == 1)
                {
                    _nextBlockScreen[y, x] = '0';
                }
                else
                {
                    _nextBlockScreen[y, x] = ' ';
                }
            }
        }

        sb.Append("Next Block\n\n");
        sb.Append("^^^^^^^^\n");

        for (int y = 0; y < 4; y++)
        {
            sb.Append("<");
            for (int x = 0; x < 4; x++)
            {
                sb.Append(" ");
                sb.Append(_nextBlockScreen[y, x]);
            }

            sb.Append(">\n");
        }

        sb.Append("--------");
        
        _nextBlockText.text = sb.ToString();
    }
    
}
