namespace Ragnarok
{
    /// <summary>
    /// A helper for flag operations with NavMeshAreas
    /// </summary>
    public struct AreaMask
    {
        private readonly int _value;

        public int Value => _value;
        public NavMeshArea Enum => (NavMeshArea)_value;

        public AreaMask(int value)
        {
            _value = value;
        }

        public AreaMask(NavMeshArea areas)
        {
            _value = (int)areas;
        }

        public static implicit operator AreaMask(int value) => new AreaMask(value);
        public static implicit operator AreaMask(string name) => new AreaMask(1 << UnityEngine.AI.NavMesh.GetAreaFromName(name));
        public static implicit operator AreaMask(NavMeshArea areas) => new AreaMask((int)areas);
        public static implicit operator NavMeshArea(AreaMask flag) => (NavMeshArea)flag._value;
        public static implicit operator int(AreaMask flag) => flag._value;

        public static bool operator ==(AreaMask a, int b) => a._value.Equals(b);
        public static bool operator !=(AreaMask a, int b) => !a._value.Equals(b);
        public static int operator +(AreaMask a, AreaMask b) => a.Add(b._value);
        public static int operator -(AreaMask a, AreaMask b) => a.Remove(b._value);
        public static int operator |(AreaMask a, AreaMask b) => a.Add(b._value);
        public static int operator ~(AreaMask a) => ~a._value;
        public static int operator +(int a, AreaMask b) => a |= b._value;
        public static int operator -(int a, AreaMask b) => a &= ~b._value;
        public static int operator |(int a, AreaMask b) => a |= b._value;
        public static int operator +(AreaMask a, int b) => a.Add(b);
        public static int operator -(AreaMask a, int b) => a.Remove(b);
        public static int operator |(AreaMask a, int b) => a.Add(b);

        public bool HasFlag(AreaMask flag) => (_value & flag._value) == flag;
        public bool HasFlag(int value) => (_value & value) == value;
        public AreaMask Add(AreaMask flag) => _value | flag._value;
        public AreaMask Remove(AreaMask flag) => _value & ~flag._value;
        public AreaMask Add(NavMeshArea flags) => _value | (int)flags;
        public AreaMask Remove(NavMeshArea flags) => _value & ~(int)flags;

        public bool Equals(AreaMask other) => _value == other._value;
        public override string ToString() => ((NavMeshArea)_value).ToString();
        public override int GetHashCode() => _value;
        public override bool Equals(object obj)
            => !ReferenceEquals(null, obj) && (obj is AreaMask other && Equals(other));
    }
}