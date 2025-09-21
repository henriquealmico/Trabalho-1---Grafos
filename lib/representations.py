from abc import ABC, abstractmethod
import numpy as np

class GraphRepresentation(ABC):
    """
    Classe base abstrata (Interface) para as representações de grafos.
    Define os métodos essenciais que toda representação de grafo deve implementar.
    """
    def __init__(self, num_vertices: int):
        self.num_vertices = num_vertices

    @abstractmethod
    def add_edge(self, u: int, v: int):
        """Adiciona uma aresta entre os vértices u e v."""
        pass

    @abstractmethod
    def get_neighbors(self, v: int) -> list[int]:
        """Retorna uma lista de vizinhos do vértice v."""
        pass

    @property
    @abstractmethod
    def edge_count(self) -> int:
        """Retorna o número total de arestas."""
        pass


class AdjacencyList(GraphRepresentation):
    """Representação de grafo utilizando lista de adjacência."""
    def __init__(self, num_vertices: int):
        super().__init__(num_vertices)
        # Usamos um dicionário para mapear cada vértice a um conjunto de seus vizinhos.
        # Conjuntos (set) garantem inserção em O(1) e evitam arestas duplicadas.
        self._adj = {i: set() for i in range(1, num_vertices + 1)}
        self._edge_count = 0

    def add_edge(self, u: int, v: int):
        """
        Adiciona uma aresta. A verificação de existência é O(1) em média para sets.
        """
        if v not in self._adj[u]:
            self._adj[u].add(v)
            self._adj[v].add(u)
            self._edge_count += 1

    def get_neighbors(self, v: int) -> list[int]:
        """Retorna os vizinhos de um vértice. A conversão para lista é feita aqui."""
        return list(self._adj[v])

    @property
    def edge_count(self) -> int:
        return self._edge_count


class AdjacencyMatrix(GraphRepresentation):
    """
    Representação de grafo utilizando matriz de adjacência com NumPy para performance.
    """
    def __init__(self, num_vertices: int):
        super().__init__(num_vertices)
        # Usamos `uint8` para otimizar o uso de memória (0 ou 1).
        # Para grafos muito grandes, isso representa uma economia significativa.
        # A matriz é indexada de 0 a n-1, então ajustamos os vértices (1 a n).
        self._matrix = np.zeros((num_vertices, num_vertices), dtype=np.uint8)
        self._edge_count = 0

    def add_edge(self, u: int, v: int):
        """
        Adiciona uma aresta. O acesso e a atribuição em uma matriz NumPy são O(1).
        """
        # Ajusta os vértices para o índice da matriz (0 a n-1)
        u_idx, v_idx = u - 1, v - 1
        if self._matrix[u_idx, v_idx] == 0:
            self._matrix[u_idx, v_idx] = 1
            self._matrix[v_idx, u_idx] = 1
            self._edge_count += 1

    def get_neighbors(self, v: int) -> list[int]:
        """
        Retorna vizinhos. np.nonzero é uma operação otimizada para encontrar
        índices de elementos não-zero.
        """
        # Ajusta o vértice para o índice da matriz
        v_idx = v - 1
        # Encontra os vizinhos e ajusta de volta para o ID do vértice (1 a n)
        neighbors_indices = np.nonzero(self._matrix[v_idx])[0]
        return (neighbors_indices + 1).tolist()

    @property
    def edge_count(self) -> int:
        return self._edge_count