using UnityEngine;
using UnityEngine.Tilemaps;

namespace AlanZucconi.Snake
{
    [CreateAssetMenu(fileName = "Snake Skin", menuName = "Snake/Snake Skin")]
    public class SnakeSkin : ScriptableObject
    {
        [Header("Head")]
        public Tile HeadNorth;
        public Tile HeadSouth;
        public Tile HeadWest;
        public Tile HeadEast;

        [Header("Tail")]
        public Tile TailNorth;
        public Tile TailSouth;
        public Tile TailWest;
        public Tile TailEast;

        [Header("Straight")]
        public Tile Horizontal;
        public Tile Vertical;

        [Header("Corners")]
        public Tile NE;
        public Tile SE;
        public Tile NW;
        public Tile SW;
    }
}