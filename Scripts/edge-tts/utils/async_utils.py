import asyncio
from typing import Callable, TypeVar, Any
import time

T = TypeVar('T')  # Generic return type

try:
    import nest_asyncio
    nest_asyncio.apply()
except ImportError:
    pass

def run_async(func: Callable[..., Any], *args, retries: int = 5, delay: float = 1.0, **kwargs) -> Any:
    """
    Run an async function synchronously with retry logic.
    Detects if there's already an active event loop.
    """
    async def retry_wrapper():
        for attempt in range(1, retries + 1):
            try:
                return await func(*args, **kwargs)
            except Exception as e:
                if attempt < retries:
                    await asyncio.sleep(delay)
                else:
                    raise Exception(f"Failed after {retries} attempts: {e}")

    loop = asyncio.get_event_loop()
    return loop.run_until_complete(retry_wrapper())