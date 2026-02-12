/// <summary>
/// 입력 검증을 담당하는 클래스입니다.
/// </summary>
public static class InputValidator
{
    /// <summary>
    /// 입력 텍스트를 검증합니다.
    /// </summary>
    /// <param name="text">검증할 텍스트</param>
    /// <returns>유효한 입력이면 true, 그렇지 않으면 false</returns>
    public static bool ValidateInput(string text)
    {
        return !string.IsNullOrWhiteSpace(text);
    }
}

