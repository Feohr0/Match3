using UnityEngine;
using System.Collections;

namespace Match3Game
{
    public class Gem : MonoBehaviour
    {

        private int x;
        private int y;

        public int X
        {
            get { return x; }
            set
            {
                if (IsMovable())
                {
                    x = value;
                }
            }
        }

        public int Y
        {
            get { return y; }
            set
            {
                if (IsMovable())
                {
                    y = value;
                }
            }
        }

        private GridSystem.GemType gemType;

        public GridSystem.GemType GemType
        {
            get { return gemType; }

        }

        private GridSystem grid;

        public GridSystem GridRef
        {
            get { return grid; }
        }

        private MovableGem movableComponent;
        public MovableGem MovableComponent
        {
            get { return movableComponent; }
        }

        private ColorGem colorComponent;
        public ColorGem ColorComponent
        {
            get { return colorComponent; }
        }

        private ClearableGem clearableComponent;

        public ClearableGem ClearableComponent
        {
            get { return clearableComponent; }
        }

        private void Awake()
        {
            movableComponent = GetComponent<MovableGem>();
            colorComponent = GetComponent<ColorGem>();
            clearableComponent = GetComponent<ClearableGem>();
        }

        public void Init(int _x, int _y, GridSystem _grid, GridSystem.GemType _type)
        {
            x = _x;
            y = _y;
            grid = _grid;
            gemType = _type;
        }

        public bool IsMovable()
        {
            return movableComponent != null;
        }

        public bool IsColored()
        {
            return colorComponent != null;
        }

        public bool IsClearable()
        {
            return clearableComponent != null;
        }

        // Mouse event'leri artık GridSystem tarafından handle ediliyor
        // Bu methodlar geriye uyumluluk için bırakıldı ama kullanılmıyor

        private void OnMouseDown()
        {
            // Drag sistem GridSystem'de handle ediliyor
            // Bu method artık kullanılmıyor ama geriye uyumluluk için bırakıldı
        }

        private void OnMouseEnter()
        {
            // Artık kullanılmıyor
        }

        private void OnMouseUp()
        {
            // Artık kullanılmıyor
        }
    }
}