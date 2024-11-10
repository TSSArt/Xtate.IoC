// Copyright © 2019-2024 Sergii Artemenko
// 
// This file is part of the Xtate project. <https://xtate.net/>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

namespace Xtate.IoC;

public abstract class Module : IModule
{
	protected IServiceCollection Services { get; private init; } = default!;

#region Interface IModule

	void IModule.AddServices()
	{
		AddServices();
	}

	IServiceCollection IModule.Services { init => Services = value; }

#endregion

	protected abstract void AddServices();
}

public abstract class Module<TDependencyModule> : IModule where TDependencyModule : IModule, new()
{
	protected IServiceCollection Services { get; private init; } = default!;

#region Interface IModule

	void IModule.AddServices()
	{
		Services.AddModule<TDependencyModule>();

		AddServices();
	}

	IServiceCollection IModule.Services { init => Services = value; }

#endregion

	protected abstract void AddServices();
}

public abstract class Module<TDependencyModule1, TDependencyModule2> : IModule
	where TDependencyModule1 : IModule, new()
	where TDependencyModule2 : IModule, new()
{
	protected IServiceCollection Services { get; private init; } = default!;

#region Interface IModule

	void IModule.AddServices()
	{
		Services.AddModule<TDependencyModule1>();
		Services.AddModule<TDependencyModule2>();

		AddServices();
	}

	IServiceCollection IModule.Services { init => Services = value; }

#endregion

	protected abstract void AddServices();
}

public abstract class Module<TDependencyModule1, TDependencyModule2, TDependencyModule3> : IModule
	where TDependencyModule1 : IModule, new()
	where TDependencyModule2 : IModule, new()
	where TDependencyModule3 : IModule, new()
{
	protected IServiceCollection Services { get; private init; } = default!;

#region Interface IModule

	void IModule.AddServices()
	{
		Services.AddModule<TDependencyModule1>();
		Services.AddModule<TDependencyModule2>();
		Services.AddModule<TDependencyModule3>();

		AddServices();
	}

	IServiceCollection IModule.Services { init => Services = value; }

#endregion

	protected abstract void AddServices();
}

public abstract class Module<TDependencyModule1, TDependencyModule2, TDependencyModule3, TDependencyModule4> : IModule
	where TDependencyModule1 : IModule, new()
	where TDependencyModule2 : IModule, new()
	where TDependencyModule3 : IModule, new()
	where TDependencyModule4 : IModule, new()
{
	protected IServiceCollection Services { get; private init; } = default!;

#region Interface IModule

	void IModule.AddServices()
	{
		Services.AddModule<TDependencyModule1>();
		Services.AddModule<TDependencyModule2>();
		Services.AddModule<TDependencyModule3>();
		Services.AddModule<TDependencyModule4>();

		AddServices();
	}

	IServiceCollection IModule.Services { init => Services = value; }

#endregion

	protected abstract void AddServices();
}

public abstract class Module<TDependencyModule1, TDependencyModule2, TDependencyModule3, TDependencyModule4, TDependencyModule5> : IModule
	where TDependencyModule1 : IModule, new()
	where TDependencyModule2 : IModule, new()
	where TDependencyModule3 : IModule, new()
	where TDependencyModule4 : IModule, new()
	where TDependencyModule5 : IModule, new()
{
	protected IServiceCollection Services { get; private init; } = default!;

#region Interface IModule

	void IModule.AddServices()
	{
		Services.AddModule<TDependencyModule1>();
		Services.AddModule<TDependencyModule2>();
		Services.AddModule<TDependencyModule3>();
		Services.AddModule<TDependencyModule4>();
		Services.AddModule<TDependencyModule5>();

		AddServices();
	}

	IServiceCollection IModule.Services { init => Services = value; }

#endregion

	protected abstract void AddServices();
}
