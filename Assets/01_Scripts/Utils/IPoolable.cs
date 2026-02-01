/// <summary>
/// 반납 자동화 하려고 만든 인터페이스
/// </summary>

public interface IPoolable {
    void OnPoolRelease();
}
