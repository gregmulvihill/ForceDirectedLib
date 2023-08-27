using CommunityToolkit.Mvvm.ComponentModel;

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ForceDirectedLibDemo.ViewModel
{
	public class ViewModelBase : ObservableObject
	{
		private readonly Dictionary<string, object> _map = new();

		public T Get<T>([CallerMemberName] string? name = null)
		{
			if (_map.TryGetValue(name!, out object? value))
			{
				return (T)value;
			}

			return default!;
		}

		public void Set<T>(T value, [CallerMemberName] string? name = null) => _map[name!] = value!;
	}
}
