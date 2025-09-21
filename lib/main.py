from graph import Graph
from representations import AdjacencyList, AdjacencyMatrix
import time

def generate_output(graph: Graph, graph_name: str):
    """Gera a saída de informações sobre o grafo, conforme solicitado."""
    print(f"--- Análise do Grafo: {graph_name} ---")
    
    # 1. Informações básicas
    print(f"Número de vértices: {graph.vertex_count}")
    print(f"Número de arestas: {graph.edge_count}")
    
    # 2. Métricas de Grau
    metrics = graph.get_degree_metrics()
    print(f"Grau Mínimo: {metrics['min_degree']}")
    print(f"Grau Máximo: {metrics['max_degree']}")
    print(f"Grau Médio: {metrics['avg_degree']:.2f}")
    print(f"Mediana de Grau: {metrics['median_degree']}")
    print("-" * 20)

    # 3. Componentes Conexas
    print("Componentes Conexas:")
    components = graph.get_connected_components()
    print(f"  Número de componentes: {len(components)}")
    for i, comp in enumerate(components):
        print(f"  - Componente {i+1}: Tamanho={comp['size']}, Vértices={comp['vertices'][:15]}...") # Mostra apenas os 15 primeiros
    print("-" * 20)

    # 4. BFS a partir de um vértice de exemplo (ex: 1)
    print("Exemplo de BFS (a partir do vértice 1):")
    start_time = time.perf_counter()
    parents_bfs, levels_bfs = graph.breadth_first_search(1)
    end_time = time.perf_counter()
    print(f"  Tempo de execução da BFS: {(end_time - start_time) * 1000:.4f} ms")
    print(f"  Pai do vértice 5 na árvore BFS: {parents_bfs.get(5)}")
    print(f"  Nível do vértice 5 na árvore BFS: {levels_bfs.get(5)}")
    print("-" * 20)
    
    # 5. DFS a partir de um vértice de exemplo (ex: 1)
    print("Exemplo de DFS (a partir do vértice 1):")
    start_time = time.perf_counter()
    parents_dfs, levels_dfs = graph.depth_first_search(1)
    end_time = time.perf_counter()
    print(f"  Tempo de execução da DFS: {(end_time - start_time) * 1000:.4f} ms")
    print(f"  Pai do vértice 5 na árvore DFS: {parents_dfs.get(5)}")
    print(f"  Nível do vértice 5 na árvore DFS: {levels_dfs.get(5)}")
    print("-" * 20)
    
    # 6. Distância entre vértices
    print(f"Distância entre 1 e 4: {graph.get_distance(1, 4)}")
    print("-" * 20)

    # 7. Diâmetro do Grafo (pode ser lento)
    print("Calculando o diâmetro...")
    start_time = time.perf_counter()
    diameter = graph.get_diameter()
    end_time = time.perf_counter()
    print(f"  Tempo de cálculo do diâmetro: {(end_time - start_time):.4f} s")
    print(f"  Diâmetro do grafo: {diameter}")
    print("\n")


if __name__ == "__main__":
    # Executa a análise com Lista de Adjacência
    graph_adj_list = Graph.from_file('./test_cases/grafo_1.txt', AdjacencyList)
    generate_output(graph_adj_list, "Grafo com Lista de Adjacência")
    
    # Executa a análise com Matriz de Adjacência
    graph_adj_matrix = Graph.from_file('./test_cases/grafo_1.txt', AdjacencyMatrix)
    generate_output(graph_adj_matrix, "Grafo com Matriz de Adjacência")

