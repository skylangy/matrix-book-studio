from typing import Protocol, Type, TypeVar, Dict, Any, Optional

T = TypeVar('T')

class IServiceCollection(Protocol):
    def register_service(self, service_type: Type[T], service_instance: T, name: Optional[str] = None):
        """Register a service with its type and optional name."""
        ...

    def get_service(self, service_type: Type[T], name: Optional[str] = None) -> T:
        """Retrieve a service by its type and optional name."""
        ...

class ServiceCollection(IServiceCollection):
    def __init__(self):
        self._services: Dict[Type, Dict[Optional[str], Any]] = {}

    def register_service(self, service_type: Type[T], service_instance: T, name: Optional[str] = None):
        if service_type not in self._services:
            self._services[service_type] = {}
        self._services[service_type][name] = service_instance

    def get_service(self, service_type: Type[T], name: Optional[str] = None) -> T:
        if service_type not in self._services or name not in self._services[service_type]:
            raise ValueError(f"Service of type {service_type} with name '{name}' is not registered.")
        return self._services[service_type][name]