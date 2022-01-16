# NautaManager
A simple WPF application to easily manage your Nauta accounts. It was made for fun and to practice some design patterns in WPF. This software has the following features:

- Multiple accounts in a single application.
- Simple interface. No extra decorations.
- Fast swap between accounts.
- Store sessions which means that your accounts wont get hanged up if something happens while connected.
- There are also extra features that you can easily add! I added an available credit and time update, but it is easy to implement more :D

## How to add features
### New Extra items
It is as fast as implement a new class (I recommend inside ExtraItems folder to keep an organized project) and inherit from `ExtraMenuFeature` abstract class. Implement
the abstractions and you are ready to go. Moreover, you can use dependency injection to bring some scoped type inside your module!

```
    class NewItem : ExtraMenuFeature
    {
        # IUserManager and IUsersRepository could be added using DI
        IUserManager Manager;
        public NewItem(IUserManager manager) : base("Menu Item Header")
        {
            Manager = manager;
        }

        public override async Task<string> ProcessClickAction(object sender, RoutedEventArgs args, UserSession selectedUser)
        {
            # Whatever you want to do once the menu item is clicked
        }
    }
```

The abstract class have a dictionary where you can change the initial status of the new `MenuItem` by using their property name as key and the property initial value in
the key associated value.

### DI types
Go to `App.xaml.cs` and add as many DI types to include in the IoC container as you want, with the desired scopes and etc.

## Issues
- There are some spanish and english text mixed up... Sorry...
- I belive that the logout error message could contain undesired text (something like `alert('...')`).
- Some unexpected behavior could happen. I have not stressed the application.

## What could be added?
- Some unit testing for the implemented features and functions.
- A more intuitive interface.
- Whatever the future brings to us...
