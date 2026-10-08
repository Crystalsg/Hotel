using Hotel.Storage;
using Hotel.Models;
using System;

namespace Hotel.Services;

public class LoginService
{
    private readonly UserStore _userRepo;
    private static readonly Dictionary<string, (int fails, DateTime? blockUntil)> _blocked = new();

    public LoginService(UserStore userRepo)
    {
        _userRepo = userRepo;
    }

    public User Authenticate(string login, string password)
    {
        if (_blocked.TryGetValue(login, out var data) && data.blockUntil.HasValue && DateTime.Now < data.blockUntil.Value)
            throw new InvalidOperationException("Вход временно заблокирован. Попробуйте через 20 секунд.");

        var user = _userRepo.FindByLogin(login);
        if (user == null || !SecurityHelper.Verify(password, user.PasswordSalt, user.PasswordHash))
        {
            RegisterFailure(login);
            throw new InvalidOperationException("Неверный логин или пароль.");
        }

        if (!user.IsActive)
        {
            RegisterFailure(login);
            throw new InvalidOperationException("Учётная запись деактивирована.");
        }

        _blocked.Remove(login);
        _userRepo.LogLoginAttempt(login, true, "Успешный вход");
        return user;
    }

    private void RegisterFailure(string login)
    {
        var current = _blocked.ContainsKey(login) ? _blocked[login] : (0, null);
        int fails = current.fails + 1;
        DateTime? blockUntil = null;
        if (fails >= 3)
        {
            blockUntil = DateTime.Now.AddSeconds(20); 
            fails = 0;
        }
        _blocked[login] = (fails, blockUntil);
        _userRepo.LogLoginAttempt(login, false, "Неверный логин или пароль");
    }
}