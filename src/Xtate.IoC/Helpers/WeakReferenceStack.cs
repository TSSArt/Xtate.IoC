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

/// <summary>
///     Stack of weak references with auto-shrinking collected objects.
/// </summary>
/// <remarks>
///     All public members are thread-safe and may be used concurrently from multiple threads.
/// </remarks>
internal class WeakReferenceStack
{
	private int _counter;

	private WeakReferenceNode? _node;

	/// <summary>
	///     Adds an object to the stack.
	/// </summary>
	/// <param name="instance">The object to add.</param>
	public void Push(object instance)
	{
		if (instance is null)
		{
			return;
		}

		if (-- _counter < -8)
		{
			_counter = RemoveOrphanedNodes(ref _node);
		}

		var newNode = new WeakReferenceNode(instance, _node);

		while (Interlocked.CompareExchange(ref _node, newNode, newNode.NextNode) != newNode.NextNode)
		{
			newNode.NextNode = _node;
		}
	}

	/// <summary>
	///     Removes orphaned nodes from the stack.
	/// </summary>
	/// <param name="node">The starting node.</param>
	/// <returns>The number of removed nodes.</returns>
	private static int RemoveOrphanedNodes(ref WeakReferenceNode? node)
	{
		var count = 0;

		while (ProcessNode(ref node))
		{
			count ++;

			if (node is { } tmpNode)
			{
				node = ref tmpNode.NextNode;
			}
			else
			{
				break;
			}
		}

		return count;
	}

	/// <summary>
	///     Processes a node to check if it is alive.
	/// </summary>
	/// <param name="node">The node to process.</param>
	/// <returns>True if the node is alive; otherwise, false.</returns>
	private static bool ProcessNode(ref WeakReferenceNode? node)
	{
		while (true)
		{
			var initNode = node;

			if (initNode is null)
			{
				return false;
			}

			if (initNode.IsAlive)
			{
				return true;
			}

			WeakReferenceNode? newNode = default;

			for (var iNode = initNode.NextNode; iNode is not null; iNode = iNode.NextNode)
			{
				if (iNode.IsAlive)
				{
					newNode = iNode;

					break;
				}
			}

			if (Interlocked.CompareExchange(ref node, newNode, initNode) == initNode)
			{
				return newNode is not null;
			}
		}
	}

	/// <summary>
	///     Tries to pop first not collected object from the stack.
	/// </summary>
	/// <param name="instance">The object taken from the stack, if successful.</param>
	/// <returns>True if an object was successfully taken; otherwise, false.</returns>
	public bool TryPop([NotNullWhen(true)] out object? instance)
	{
		while (true)
		{
			var initNode = _node;
			instance = default;

			if (initNode is null)
			{
				return false;
			}

			WeakReferenceNode? newNode = default;

			for (var iNode = initNode; iNode is not null; iNode = iNode.NextNode)
			{
				if (iNode.Target is { } target)
				{
					newNode = iNode.NextNode;
					instance = target;

					break;
				}
			}

			if (Interlocked.CompareExchange(ref _node, newNode, initNode) == initNode)
			{
				return instance is not null;
			}
		}
	}

	/// <summary>
	///     Represents a node in the weak reference collection.
	/// </summary>
	private class WeakReferenceNode(object target, WeakReferenceNode? nextNode) : WeakReference(target)
	{
		/// <summary>
		///     Gets or sets the next node in the collection.
		/// </summary>
		public WeakReferenceNode? NextNode = nextNode;
	}
}